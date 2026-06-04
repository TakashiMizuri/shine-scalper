using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShineScalper.Core.Enums;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;
using ShineScalper.Engine.Execution;

namespace ShineScalper.Engine;

public sealed class TradingOrchestrator : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISignalEngine _signalEngine;
    private readonly IMarketDataStream _marketData;
    private readonly TradingOptions _trading;
    private readonly RiskOptions _risk;
    private readonly ILogger<TradingOrchestrator> _logger;

    private readonly Dictionary<Guid, Position> _openPositions = new();
    private readonly object _lock = new();

    public TradingOrchestrator(
        IServiceScopeFactory scopeFactory,
        ISignalEngine signalEngine,
        IMarketDataStream marketData,
        IOptions<TradingOptions> trading,
        IOptions<RiskOptions> risk,
        ILogger<TradingOrchestrator> logger)
    {
        _scopeFactory = scopeFactory;
        _signalEngine = signalEngine;
        _marketData = marketData;
        _trading = trading.Value;
        _risk = risk.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await LoadOpenPositionsAsync(stoppingToken);

        _signalEngine.SignalGenerated += OnSignal;
        _marketData.TickerUpdated += OnTicker;

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        finally
        {
            _signalEngine.SignalGenerated -= OnSignal;
            _marketData.TickerUpdated -= OnTicker;
        }
    }

    private async Task LoadOpenPositionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var trades = scope.ServiceProvider.GetRequiredService<ITradeRepository>();
        var open = await trades.GetOpenPositionsAsync(ct);
        lock (_lock)
        {
            foreach (var p in open)
                _openPositions[p.Id] = p;
        }
    }

    private void OnSignal(TradeSignal signal)
    {
        _ = HandleSignalAsync(signal);
    }

    private async Task HandleSignalAsync(TradeSignal signal)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var risk = sp.GetRequiredService<IRiskManager>();
            var executor = ResolveExecutor(sp);
            var trades = sp.GetRequiredService<ITradeRepository>();
            var publisher = sp.GetRequiredService<ITradingEventPublisher>();

            if (!risk.CanOpenPosition(signal.Symbol, out var reject))
            {
                var rejected = RejectSignal(signal, reject);
                await trades.SaveSignalAsync(rejected);
                await publisher.PublishSignalAsync(rejected);
                return;
            }

            if (!risk.TryBuildPosition(signal, signal.TriggerPrice, out var position, out reject) || position is null)
            {
                var rejected = RejectSignal(signal, reject);
                await trades.SaveSignalAsync(rejected);
                await publisher.PublishSignalAsync(rejected);
                return;
            }

            await trades.SaveSignalAsync(signal);
            await executor.OpenPositionAsync(position, signal.TriggerPrice);
            await trades.SavePositionAsync(position);

            lock (_lock) _openPositions[position.Id] = position;

            await publisher.PublishSignalAsync(signal);
            await publisher.PublishPositionAsync(position);

            var fill = await trades.GetFillsAsync(1);
            if (fill.Count > 0)
                await publisher.PublishTradeAsync(fill[0]);

            _logger.LogInformation("Opened {Side} position {Id} @ {Price}", position.Side, position.Id, position.EntryPrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle signal");
        }
    }

    private void OnTicker(string symbol, decimal price)
    {
        _signalEngine.OnTicker(symbol, price, DateTime.UtcNow);
        _ = ManagePositionsAsync(symbol, price);
    }

    private async Task ManagePositionsAsync(string symbol, decimal price)
    {
        List<Position> positions;
        lock (_lock)
        {
            positions = _openPositions.Values.Where(p => p.Symbol == symbol).ToList();
        }

        foreach (var position in positions)
            await ManagePositionAsync(position, price);
    }

    private async Task ManagePositionAsync(Position position, decimal price)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var executor = ResolveExecutor(sp);
            var trades = sp.GetRequiredService<ITradeRepository>();
            var publisher = sp.GetRequiredService<ITradingEventPublisher>();
            var risk = sp.GetRequiredService<IRiskManager>();

            if (IsStopHit(position, price))
            {
                await ClosePositionAsync(position, price, executor, trades, publisher, risk, "Stop loss");
                return;
            }

            while (position.PartialTpIndex < position.TakeProfitPrices.Count)
            {
                var tp = position.TakeProfitPrices[position.PartialTpIndex];
                if (!IsTpHit(position, price, tp))
                    break;

                var partialQty = position.Quantity * _risk.PartialTpPercent / 100m;
                var fill = await executor.ClosePartialAsync(position, partialQty, price, position.PartialTpIndex);
                if (fill is null) break;

                position.PartialTpIndex++;
                await trades.UpdatePositionAsync(position);
                await publisher.PublishTradeAsync(fill);
                await publisher.PublishPositionAsync(position);

                if (position.RemainingQuantity <= 0)
                {
                    position.Status = PositionStatus.Closed;
                    position.ClosedAtUtc = DateTime.UtcNow;
                    await trades.UpdatePositionAsync(position);
                    lock (_lock) _openPositions.Remove(position.Id);
                    await publisher.PublishPositionAsync(position);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Position management error {Id}", position.Id);
        }
    }

    private async Task ClosePositionAsync(
        Position position,
        decimal price,
        IOrderExecutor executor,
        ITradeRepository trades,
        ITradingEventPublisher publisher,
        IRiskManager risk,
        string reason)
    {
        var fill = await executor.CloseFullAsync(position, price, reason);
        risk.RecordDailyPnL(position.RealizedPnL);
        lock (_lock) _openPositions.Remove(position.Id);
        await publisher.PublishTradeAsync(fill);
        await publisher.PublishPositionAsync(position);
        _logger.LogInformation("Closed position {Id}: {Reason}, PnL={PnL}", position.Id, reason, position.RealizedPnL);
    }

    private static bool IsStopHit(Position p, decimal price) =>
        p.Side switch
        {
            TradeSide.Long => price <= p.StopLoss,
            TradeSide.Short => price >= p.StopLoss,
            _ => false
        };

    private static bool IsTpHit(Position p, decimal price, decimal tp) =>
        p.Side switch
        {
            TradeSide.Long => price >= tp,
            TradeSide.Short => price <= tp,
            _ => false
        };

    private IOrderExecutor ResolveExecutor(IServiceProvider sp) =>
        _trading.Mode == TradingMode.Live
            ? sp.GetRequiredService<LiveOrderExecutor>()
            : sp.GetRequiredService<PaperOrderExecutor>();

    private static TradeSignal RejectSignal(TradeSignal signal, string? reason) => new()
    {
        Id = signal.Id,
        Symbol = signal.Symbol,
        Side = signal.Side,
        LevelId = signal.LevelId,
        Level = signal.Level,
        TriggerPrice = signal.TriggerPrice,
        Reason = signal.Reason,
        Status = SignalStatus.Rejected,
        RejectReason = reason,
        TimestampUtc = signal.TimestampUtc
    };
}

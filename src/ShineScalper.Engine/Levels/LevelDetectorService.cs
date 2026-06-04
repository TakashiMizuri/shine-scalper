using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;
using ShineScalper.Core.Services;

namespace ShineScalper.Engine.Levels;

public sealed class LevelDetectorService : BackgroundService, ILevelDetector
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TradingStateCache _cache;
    private readonly TradingOptions _trading;
    private readonly LevelOptions _levels;
    private readonly IMarketDataStream _marketData;
    private readonly ILogger<LevelDetectorService> _logger;
    private IReadOnlyList<PriceZone> _active = [];

    public LevelDetectorService(
        IServiceScopeFactory scopeFactory,
        TradingStateCache cache,
        IOptions<TradingOptions> trading,
        IOptions<LevelOptions> levels,
        IMarketDataStream marketData,
        ILogger<LevelDetectorService> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _trading = trading.Value;
        _levels = levels.Value;
        _marketData = marketData;
        _logger = logger;
    }

    public IReadOnlyList<PriceZone> GetActiveLevels(string symbol) =>
        _active.Where(l => l.Symbol == symbol && !l.Invalidated).ToList();

    public async Task RefreshAsync(string symbol, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var candles = scope.ServiceProvider.GetRequiredService<ICandleRepository>();
        var trades = scope.ServiceProvider.GetRequiredService<ITradeRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<ITradingEventPublisher>();

        var since = DateTime.UtcNow.AddHours(-_levels.LookbackHours);
        var candleList = await candles.GetAsync(symbol, _levels.Timeframe, since, 1000, ct);
        _active = LevelDetectorLogic.Detect(candleList, _levels, symbol);
        _cache.SetLevels(_active);
        await trades.SaveLevelSnapshotAsync(symbol, _active, ct);
        await publisher.PublishLevelsAsync(symbol, _active, ct);
        _logger.LogInformation("Detected {Count} active levels for {Symbol}", _active.Count, symbol);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken);
        await RefreshAsync(_trading.Symbol, stoppingToken);

        void OnCandle(Candle c)
        {
            if (c.Symbol != _trading.Symbol || c.Interval != _levels.Timeframe || !c.IsClosed)
                return;
            _ = RefreshSafeAsync(c.Symbol, stoppingToken);
        }

        _marketData.CandleUpdated += OnCandle;
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        finally
        {
            _marketData.CandleUpdated -= OnCandle;
        }
    }

    private async Task RefreshSafeAsync(string symbol, CancellationToken ct)
    {
        try
        {
            using var scope = CancellationTokenSource.CreateLinkedTokenSource(ct);
            await RefreshAsync(symbol, scope.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Level refresh failed for {Symbol}", symbol);
        }
    }
}

using Microsoft.Extensions.Options;
using ShineScalper.Core.Enums;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;

namespace ShineScalper.Engine.Risk;

public sealed class RiskManager : IRiskManager
{
    private readonly ITradeRepository _trades;
    private readonly ILevelDetector _levels;
    private readonly TradingOptions _trading;
    private readonly RiskOptions _risk;

    public RiskManager(
        ITradeRepository trades,
        ILevelDetector levels,
        IOptions<TradingOptions> trading,
        IOptions<RiskOptions> risk)
    {
        _trades = trades;
        _levels = levels;
        _trading = trading.Value;
        _risk = risk.Value;
    }

    public decimal GetEquity() => _trades.GetEquityAsync().GetAwaiter().GetResult();

    public bool CanOpenPosition(string symbol, out string? rejectReason)
    {
        rejectReason = null;

        if (_trading.Mode == TradingMode.Live)
        {
            rejectReason = "Live trading not enabled in v0.1";
            return false;
        }

        var openCount = _trades.CountOpenPositionsAsync().GetAwaiter().GetResult();
        if (openCount >= _trading.MaxOpenPositions)
        {
            rejectReason = "Max open positions reached";
            return false;
        }

        if (_trades.HasOpenPositionForSymbolAsync(symbol).GetAwaiter().GetResult())
        {
            rejectReason = "Position already open for symbol";
            return false;
        }

        var equity = GetEquity();
        var dailyPnl = _trades.GetDailyPnLAsync(DateTime.UtcNow).GetAwaiter().GetResult();
        var dailyLimit = equity * _trading.DailyLossLimitPercent / 100m;
        if (dailyPnl <= -dailyLimit)
        {
            rejectReason = "Daily loss limit reached";
            return false;
        }

        return true;
    }

    public bool TryBuildPosition(TradeSignal signal, decimal entryPrice, out Position? position, out string? rejectReason)
    {
        position = null;
        rejectReason = null;

        if (!CanOpenPosition(signal.Symbol, out rejectReason))
            return false;

        var sl = CalculateStopLoss(signal, entryPrice);
        var slDistance = Math.Abs(entryPrice - sl);
        if (slDistance <= 0)
        {
            rejectReason = "Invalid stop loss distance";
            return false;
        }

        var equity = GetEquity();
        var riskAmount = equity * _trading.RiskPerTradePercent / 100m;
        var qty = riskAmount / slDistance;

        var notional = qty * entryPrice;
        if (notional > _trading.MaxNotionalUsdt)
            qty = _trading.MaxNotionalUsdt / entryPrice;

        if (qty <= 0)
        {
            rejectReason = "Calculated quantity is zero";
            return false;
        }

        var tpLevels = FindTakeProfitLevels(signal, entryPrice);

        position = new Position
        {
            Symbol = signal.Symbol,
            Side = signal.Side,
            Status = PositionStatus.Open,
            EntryPrice = entryPrice,
            StopLoss = sl,
            Quantity = qty,
            RemainingQuantity = qty,
            StopLossMode = _risk.StopLossMode,
            LevelId = signal.LevelId,
            OpenedAtUtc = DateTime.UtcNow,
            TakeProfitPrices = tpLevels
        };

        return true;
    }

    public void RecordDailyPnL(decimal pnl) =>
        _trades.AddDailyPnLAsync(pnl, DateTime.UtcNow).GetAwaiter().GetResult();

    private decimal CalculateStopLoss(TradeSignal signal, decimal entryPrice)
    {
        var zone = signal.Level;
        var buffer = zone.Center * _risk.SlZoneBufferBps / 10000m;

        return signal.Side switch
        {
            TradeSide.Long => zone.LowerBound - buffer,
            TradeSide.Short => zone.UpperBound + buffer,
            _ => entryPrice
        };
    }

    private List<decimal> FindTakeProfitLevels(TradeSignal signal, decimal entryPrice)
    {
        var all = _levels.GetActiveLevels(signal.Symbol)
            .Where(z => z.Id != signal.LevelId && !z.Invalidated)
            .Select(z => z.Center)
            .ToList();

        return signal.Side switch
        {
            TradeSide.Long => all.Where(p => p > entryPrice).OrderBy(p => p).ToList(),
            TradeSide.Short => all.Where(p => p < entryPrice).OrderByDescending(p => p).ToList(),
            _ => []
        };
    }
}

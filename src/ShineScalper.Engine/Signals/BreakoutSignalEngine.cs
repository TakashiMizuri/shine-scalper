using Microsoft.Extensions.Options;
using ShineScalper.Core.Enums;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;
using ShineScalper.Core.Services;

namespace ShineScalper.Engine.Signals;

public sealed class BreakoutSignalEngine : ISignalEngine
{
    private readonly ILevelDetector _levels;
    private readonly TradingOptions _trading;
    private readonly SignalOptions _signal;
    private readonly TradingStateCache _cache;

    private readonly Dictionary<string, ZoneWatch> _watches = new();
    private readonly object _lock = new();

    public event Action<TradeSignal>? SignalGenerated;

    public BreakoutSignalEngine(
        ILevelDetector levels,
        IOptions<TradingOptions> trading,
        IOptions<SignalOptions> signal,
        TradingStateCache cache)
    {
        _levels = levels;
        _trading = trading.Value;
        _signal = signal.Value;
        _cache = cache;
    }

    public void OnTicker(string symbol, decimal lastPrice, DateTime utcNow)
    {
        if (symbol != _trading.Symbol) return;

        var activeLevels = _levels.GetActiveLevels(symbol);
        if (activeLevels.Count == 0) return;

        var buffer = lastPrice * _signal.BreakoutBufferBps / 10000m;

        foreach (var level in activeLevels)
        {
            ProcessLevel(symbol, level, lastPrice, buffer, utcNow);
        }
    }

    private void ProcessLevel(string symbol, PriceZone level, decimal price, decimal buffer, DateTime utcNow)
    {
        lock (_lock)
        {
            if (!_watches.TryGetValue(level.Id, out var watch))
            {
                watch = new ZoneWatch();
                _watches[level.Id] = watch;
            }

            if (level.ContainsPrice(price))
            {
                watch.TicksInZone++;
                watch.InZoneSince ??= utcNow;
            }
            else
            {
                if (watch.InZoneSince.HasValue &&
                    (utcNow - watch.InZoneSince.Value).TotalSeconds >= _signal.InZoneMinSeconds &&
                    watch.TicksInZone >= _signal.InZoneMinTicks)
                {
                    TryEmitBreakout(symbol, level, price, buffer, utcNow);
                }
                watch.ResetZone();
            }

            watch.Prices.Enqueue((utcNow, price));
            while (watch.Prices.Count > 0 &&
                   (utcNow - watch.Prices.Peek().Time).TotalMinutes > _signal.ApproachWindowMinutes)
                watch.Prices.Dequeue();
        }
    }

    private void TryEmitBreakout(string symbol, PriceZone level, decimal price, decimal buffer, DateTime utcNow)
    {
        TradeSide? side = null;
        if (level.Type == LevelType.Resistance && price > level.UpperBound + buffer)
            side = TradeSide.Long;
        else if (level.Type == LevelType.Support && price < level.LowerBound - buffer)
            side = TradeSide.Short;

        if (side is null) return;

        if (!HasApproach(level, side.Value)) return;

        var signal = new TradeSignal
        {
            Symbol = symbol,
            Side = side.Value,
            LevelId = level.Id,
            Level = level,
            TriggerPrice = price,
            Reason = $"Breakout {side} through {level.Type} zone",
            Status = SignalStatus.Accepted,
            TimestampUtc = utcNow
        };

        SignalGenerated?.Invoke(signal);
        _watches.Remove(level.Id);
    }

    private bool HasApproach(PriceZone level, TradeSide side)
    {
        if (!_watches.TryGetValue(level.Id, out var watch) || watch.Prices.Count < 2)
            return true;

        var oldest = watch.Prices.Peek().Price;
        var newest = watch.Prices.Last().Price;
        var distOld = Math.Abs(oldest - level.Center) / level.Center * 100m;
        var distNew = Math.Abs(newest - level.Center) / level.Center * 100m;

        return distNew <= distOld && distOld - distNew >= _signal.ApproachMinPercent;
    }

    private sealed class ZoneWatch
    {
        public Queue<(DateTime Time, decimal Price)> Prices { get; } = new();
        public DateTime? InZoneSince { get; set; }
        public int TicksInZone { get; set; }

        public void ResetZone()
        {
            InZoneSince = null;
            TicksInZone = 0;
        }
    }
}

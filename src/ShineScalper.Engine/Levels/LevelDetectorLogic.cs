using ShineScalper.Core.Enums;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;

namespace ShineScalper.Engine.Levels;

public static class LevelDetectorLogic
{
    public static IReadOnlyList<PriceZone> Detect(
        IReadOnlyList<Candle> candles,
        LevelOptions options,
        string symbol)
    {
        if (candles.Count < options.SwingLookback * 2 + 1)
            return [];

        var swings = FindSwings(candles, options.SwingLookback);
        var clusters = ClusterSwings(swings, options);
        var zones = new List<PriceZone>();

        foreach (var cluster in clusters)
        {
            var center = cluster.Prices.OrderBy(p => p).ElementAt(cluster.Prices.Count / 2);
            var width = center * options.ZoneWidthPercent / 100m;
            var lower = center - width;
            var upper = center + width;

            var (touches, lastTouch, type) = CountTouches(candles, lower, upper);
            if (touches < options.MinTouches)
                continue;

            if (options.MaxLevelAgeHours > 0 &&
                (DateTime.UtcNow - lastTouch).TotalHours > options.MaxLevelAgeHours)
                continue;

            var invalidated = type switch
            {
                LevelType.Support => candles.TakeLast(3).Any(c => c.IsClosed && c.Close < lower),
                LevelType.Resistance => candles.TakeLast(3).Any(c => c.IsClosed && c.Close > upper),
                _ => false
            };

            zones.Add(new PriceZone
            {
                Id = $"{symbol}-{lower:F4}-{upper:F4}",
                Symbol = symbol,
                Center = center,
                LowerBound = lower,
                UpperBound = upper,
                Type = type,
                Touches = touches,
                LastTouchUtc = lastTouch,
                Invalidated = invalidated,
                DetectedAtUtc = DateTime.UtcNow
            });
        }

        return zones
            .Where(z => !z.Invalidated)
            .OrderBy(z => z.Center)
            .ToList();
    }

    private static List<SwingPoint> FindSwings(IReadOnlyList<Candle> candles, int lookback)
    {
        var swings = new List<SwingPoint>();
        for (var i = lookback; i < candles.Count - lookback; i++)
        {
            var c = candles[i];
            var isHigh = true;
            var isLow = true;
            for (var j = i - lookback; j <= i + lookback; j++)
            {
                if (j == i) continue;
                if (candles[j].High >= c.High) isHigh = false;
                if (candles[j].Low <= c.Low) isLow = false;
            }

            if (isHigh)
                swings.Add(new SwingPoint(c.OpenTimeMs, c.High, true));
            if (isLow)
                swings.Add(new SwingPoint(c.OpenTimeMs, c.Low, false));
        }
        return swings;
    }

    private static List<SwingCluster> ClusterSwings(List<SwingPoint> swings, LevelOptions options)
    {
        var sorted = swings.OrderBy(s => s.Price).ToList();
        var clusters = new List<SwingCluster>();
        SwingCluster? current = null;

        foreach (var swing in sorted)
        {
            if (current is null)
            {
                current = new SwingCluster { Prices = [swing.Price], IsHigh = swing.IsHigh };
                continue;
            }

            var refPrice = current.Prices[0];
            var diffPercent = Math.Abs(swing.Price - refPrice) / refPrice * 100m;
            if (diffPercent <= options.ClusterMergePercent)
                current.Prices.Add(swing.Price);
            else
            {
                clusters.Add(current);
                current = new SwingCluster { Prices = [swing.Price], IsHigh = swing.IsHigh };
            }
        }

        if (current is not null)
            clusters.Add(current);

        return clusters;
    }

    private static (int touches, DateTime lastTouch, LevelType type) CountTouches(
        IReadOnlyList<Candle> candles, decimal lower, decimal upper)
    {
        var touches = 0;
        DateTime lastTouch = DateTime.MinValue;
        LevelType type = LevelType.Support;
        LevelType? lastBounce = null;

        foreach (var c in candles)
        {
            var touched = c.Low <= upper && c.High >= lower;
            if (!touched) continue;

            var time = DateTimeOffset.FromUnixTimeMilliseconds(c.OpenTimeMs).UtcDateTime;

            if (c.Close > upper)
            {
                touches++;
                lastTouch = time;
                lastBounce = LevelType.Support;
            }
            else if (c.Close < lower)
            {
                touches++;
                lastTouch = time;
                lastBounce = LevelType.Resistance;
            }
        }

        if (lastBounce.HasValue)
            type = lastBounce.Value;

        return (touches, lastTouch == DateTime.MinValue ? DateTime.UtcNow : lastTouch, type);
    }

    private sealed record SwingPoint(long TimeMs, decimal Price, bool IsHigh);

    private sealed class SwingCluster
    {
        public List<decimal> Prices { get; init; } = [];
        public bool IsHigh { get; init; }
    }
}

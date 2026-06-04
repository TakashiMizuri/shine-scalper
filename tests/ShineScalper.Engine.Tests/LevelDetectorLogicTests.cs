using ShineScalper.Core.Enums;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;
using ShineScalper.Engine.Levels;

namespace ShineScalper.Engine.Tests;

public class LevelDetectorLogicTests
{
    [Fact]
    public void Detect_ReturnsSupportLevel_WhenTouchesAndSwingsAlign()
    {
        var options = new LevelOptions
        {
            ZoneWidthPercent = 1.0m,
            MinTouches = 3,
            SwingLookback = 2,
            ClusterMergePercent = 1.0m,
            MaxLevelAgeHours = 0
        };

        var candles = BuildSupportBounceCandles();
        var levels = LevelDetectorLogic.Detect(candles, options, "SOLUSDT");

        Assert.NotEmpty(levels);
        Assert.Contains(levels, l => l.Type == LevelType.Support && l.Touches >= 3);
    }

    /// <summary>
    /// Creates 5m candles with periodic dips to ~100 and closes back above 101 (support bounce).
    /// </summary>
    private static List<Candle> BuildSupportBounceCandles()
    {
        var list = new List<Candle>();
        var startMs = DateTimeOffset.Parse("2026-01-01T00:00:00Z").ToUnixTimeMilliseconds();

        for (var i = 0; i < 60; i++)
        {
            var isTouch = i % 6 == 0;
            decimal low, high, close, open;

            if (isTouch)
            {
                low = 99.5m;
                high = 100.5m;
                close = 101.2m;
                open = 100.5m;
            }
            else
            {
                low = 103m;
                high = 108m;
                close = 106m;
                open = 105m;
            }

            list.Add(new Candle
            {
                Symbol = "SOLUSDT",
                Interval = "5",
                OpenTimeMs = startMs + i * 300_000L,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 1000,
                IsClosed = true
            });
        }

        return list;
    }
}

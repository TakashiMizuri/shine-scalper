using Microsoft.Extensions.Options;
using Moq;
using ShineScalper.Core.Enums;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;
using ShineScalper.Engine.Risk;

namespace ShineScalper.Engine.Tests;

public class RiskManagerTests
{
    [Fact]
    public void TryBuildPosition_CalculatesQuantity_FromRiskPercent()
    {
        var trades = new Mock<ITradeRepository>();
        trades.Setup(t => t.GetEquityAsync(It.IsAny<CancellationToken>())).ReturnsAsync(10_000m);
        trades.Setup(t => t.CountOpenPositionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        trades.Setup(t => t.HasOpenPositionForSymbolAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        trades.Setup(t => t.GetDailyPnLAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(0m);

        var levels = new Mock<ILevelDetector>();
        levels.Setup(l => l.GetActiveLevels(It.IsAny<string>())).Returns([]);

        var manager = new RiskManager(
            trades.Object,
            levels.Object,
            Options.Create(new TradingOptions { RiskPerTradePercent = 2, MaxNotionalUsdt = 50_000 }),
            Options.Create(new RiskOptions()));

        var zone = new PriceZone
        {
            Id = "z1",
            Symbol = "SOLUSDT",
            Center = 100,
            LowerBound = 99,
            UpperBound = 101,
            Type = LevelType.Resistance,
            Touches = 3,
            LastTouchUtc = DateTime.UtcNow,
            DetectedAtUtc = DateTime.UtcNow
        };

        var signal = new TradeSignal
        {
            Symbol = "SOLUSDT",
            Side = TradeSide.Long,
            LevelId = zone.Id,
            Level = zone,
            TriggerPrice = 102,
            Reason = "test",
            Status = SignalStatus.Accepted,
            TimestampUtc = DateTime.UtcNow
        };

        var ok = manager.TryBuildPosition(signal, 102, out var position, out _);

        Assert.True(ok);
        Assert.NotNull(position);
        // risk 200 / sl distance ~3 => qty ~ 66
        Assert.True(position!.Quantity > 50);
    }
}

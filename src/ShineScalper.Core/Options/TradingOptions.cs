using ShineScalper.Core.Enums;

namespace ShineScalper.Core.Options;

public sealed class TradingOptions
{
    public const string SectionName = "Trading";

    public TradingMode Mode { get; set; } = TradingMode.Paper;
    public string Exchange { get; set; } = "Bybit";
    public string Symbol { get; set; } = "SOLUSDT";
    public int Leverage { get; set; } = 10;
    public decimal RiskPerTradePercent { get; set; } = 2m;
    public int MaxOpenPositions { get; set; } = 5;
    public decimal DailyLossLimitPercent { get; set; } = 6m;
    public decimal InitialPaperEquity { get; set; } = 10_000m;
    public decimal MaxNotionalUsdt { get; set; } = 5_000m;
}

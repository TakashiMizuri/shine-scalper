namespace ShineScalper.Core.Options;

public sealed class SignalOptions
{
    public const string SectionName = "Signal";

    public string Strategy { get; set; } = "Breakout";
    public int ApproachWindowMinutes { get; set; } = 15;
    public decimal ApproachMinPercent { get; set; } = 0.15m;
    public int InZoneMinSeconds { get; set; } = 5;
    public int InZoneMinTicks { get; set; } = 10;
    public decimal BreakoutBufferBps { get; set; } = 5m;
    public decimal BreakoutVolumeMultiplier { get; set; } = 1.5m;
}

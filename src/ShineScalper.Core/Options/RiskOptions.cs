using ShineScalper.Core.Enums;

namespace ShineScalper.Core.Options;

public sealed class RiskOptions
{
    public const string SectionName = "Risk";

    public StopLossMode StopLossMode { get; set; } = StopLossMode.BehindZone;
    public int AtrPeriod { get; set; } = 14;
    public decimal AtrMultiplier { get; set; } = 1.5m;
    public decimal SlZoneBufferBps { get; set; } = 10m;
    public decimal PartialTpPercent { get; set; } = 25m;
}

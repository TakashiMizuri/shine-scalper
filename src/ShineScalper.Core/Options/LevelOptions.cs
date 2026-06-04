namespace ShineScalper.Core.Options;

public sealed class LevelOptions
{
    public const string SectionName = "Levels";

    public string Timeframe { get; set; } = "5";
    public int LookbackHours { get; set; } = 48;
    public decimal ZoneWidthPercent { get; set; } = 0.25m;
    public int MinTouches { get; set; } = 3;
    public int MaxLevelAgeHours { get; set; } = 36;
    public int SwingLookback { get; set; } = 2;
    public decimal ClusterMergePercent { get; set; } = 0.15m;
}

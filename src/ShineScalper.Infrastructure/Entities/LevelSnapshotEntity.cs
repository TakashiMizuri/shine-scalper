namespace ShineScalper.Infrastructure.Entities;

public sealed class LevelSnapshotEntity
{
    public long Id { get; set; }
    public required string Symbol { get; set; }
    public required string LevelId { get; set; }
    public decimal Center { get; set; }
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }
    public int LevelType { get; set; }
    public int Touches { get; set; }
    public DateTime LastTouchUtc { get; set; }
    public bool Invalidated { get; set; }
    public DateTime DetectedAtUtc { get; set; }
    public DateTime SnapshotAtUtc { get; set; }
}

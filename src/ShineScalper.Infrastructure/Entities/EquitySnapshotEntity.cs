namespace ShineScalper.Infrastructure.Entities;

public sealed class EquitySnapshotEntity
{
    public long Id { get; set; }
    public decimal Equity { get; set; }
    public DateTime TimestampUtc { get; set; }
}

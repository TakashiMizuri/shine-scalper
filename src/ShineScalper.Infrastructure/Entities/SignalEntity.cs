namespace ShineScalper.Infrastructure.Entities;

public sealed class SignalEntity
{
    public Guid Id { get; set; }
    public required string Symbol { get; set; }
    public int Side { get; set; }
    public required string LevelId { get; set; }
    public int Status { get; set; }
    public required string Reason { get; set; }
    public string? RejectReason { get; set; }
    public decimal TriggerPrice { get; set; }
    public DateTime TimestampUtc { get; set; }
}

namespace ShineScalper.Infrastructure.Entities;

public sealed class FillEntity
{
    public Guid Id { get; set; }
    public Guid PositionId { get; set; }
    public required string Symbol { get; set; }
    public int Side { get; set; }
    public bool IsEntry { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public decimal Fee { get; set; }
    public int? PartialTpIndex { get; set; }
    public DateTime TimestampUtc { get; set; }
}

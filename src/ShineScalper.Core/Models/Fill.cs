using ShineScalper.Core.Enums;

namespace ShineScalper.Core.Models;

public sealed class Fill
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid PositionId { get; init; }
    public required string Symbol { get; init; }
    public TradeSide Side { get; init; }
    public bool IsEntry { get; init; }
    public decimal Price { get; init; }
    public decimal Quantity { get; init; }
    public decimal Fee { get; init; }
    public int? PartialTpIndex { get; init; }
    public DateTime TimestampUtc { get; init; }
}

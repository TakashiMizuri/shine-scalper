using ShineScalper.Core.Enums;

namespace ShineScalper.Core.Models;

public sealed class PriceZone
{
    public required string Id { get; init; }
    public required string Symbol { get; init; }
    public decimal Center { get; init; }
    public decimal LowerBound { get; init; }
    public decimal UpperBound { get; init; }
    public LevelType Type { get; init; }
    public int Touches { get; init; }
    public DateTime LastTouchUtc { get; init; }
    public bool Invalidated { get; init; }
    public DateTime DetectedAtUtc { get; init; }

    public bool ContainsPrice(decimal price) => price >= LowerBound && price <= UpperBound;
}

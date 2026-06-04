namespace ShineScalper.Core.Models;

public sealed class Candle
{
    public required string Symbol { get; init; }
    public required string Interval { get; init; }
    public long OpenTimeMs { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public decimal Volume { get; init; }
    public bool IsClosed { get; init; }
}

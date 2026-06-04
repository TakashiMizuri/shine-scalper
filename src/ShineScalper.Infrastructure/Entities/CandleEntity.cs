namespace ShineScalper.Infrastructure.Entities;

public sealed class CandleEntity
{
    public long Id { get; set; }
    public required string Symbol { get; set; }
    public required string Interval { get; set; }
    public long OpenTimeMs { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public bool IsClosed { get; set; }
}

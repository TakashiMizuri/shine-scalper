namespace ShineScalper.Infrastructure.Entities;

public sealed class PositionEntity
{
    public Guid Id { get; set; }
    public required string Symbol { get; set; }
    public int Side { get; set; }
    public int Status { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal Quantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public int StopLossMode { get; set; }
    public required string LevelId { get; set; }
    public DateTime OpenedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public decimal RealizedPnL { get; set; }
    public string TakeProfitPricesJson { get; set; } = "[]";
    public int PartialTpIndex { get; set; }
}

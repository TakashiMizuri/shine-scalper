using ShineScalper.Core.Enums;

namespace ShineScalper.Core.Models;

public sealed class Position
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Symbol { get; init; }
    public TradeSide Side { get; init; }
    public PositionStatus Status { get; set; }
    public decimal EntryPrice { get; init; }
    public decimal StopLoss { get; init; }
    public decimal Quantity { get; init; }
    public decimal RemainingQuantity { get; set; }
    public StopLossMode StopLossMode { get; init; }
    public required string LevelId { get; init; }
    public DateTime OpenedAtUtc { get; init; }
    public DateTime? ClosedAtUtc { get; set; }
    public decimal RealizedPnL { get; set; }
    public List<decimal> TakeProfitPrices { get; init; } = [];
    public int PartialTpIndex { get; set; }
}

using ShineScalper.Core.Enums;

namespace ShineScalper.Core.Models;

public sealed class TradeSignal
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Symbol { get; init; }
    public TradeSide Side { get; init; }
    public required string LevelId { get; init; }
    public required PriceZone Level { get; init; }
    public decimal TriggerPrice { get; init; }
    public required string Reason { get; init; }
    public SignalStatus Status { get; init; }
    public string? RejectReason { get; init; }
    public DateTime TimestampUtc { get; init; }
}

using ShineScalper.Core.Enums;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;

namespace ShineScalper.Api.Dtos;

public sealed record CandleDto(
    string Symbol,
    string Interval,
    long Time,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    bool IsClosed)
{
    public static CandleDto From(Candle c) => new(
        c.Symbol, c.Interval, c.OpenTimeMs / 1000,
        c.Open, c.High, c.Low, c.Close, c.Volume, c.IsClosed);
}

public sealed record LevelDto(
    string Id,
    string Symbol,
    decimal Center,
    decimal LowerBound,
    decimal UpperBound,
    string Type,
    int Touches,
    bool Invalidated)
{
    public static LevelDto From(PriceZone z) => new(
        z.Id, z.Symbol, z.Center, z.LowerBound, z.UpperBound,
        z.Type.ToString(), z.Touches, z.Invalidated);
}

public sealed record SignalDto(
    Guid Id,
    string Symbol,
    string Side,
    string LevelId,
    string Status,
    string Reason,
    string? RejectReason,
    decimal TriggerPrice,
    DateTime TimestampUtc)
{
    public static SignalDto From(TradeSignal s) => new(
        s.Id, s.Symbol, s.Side.ToString(), s.LevelId,
        s.Status.ToString(), s.Reason, s.RejectReason,
        s.TriggerPrice, s.TimestampUtc);
}

public sealed record PositionDto(
    Guid Id,
    string Symbol,
    string Side,
    string Status,
    decimal EntryPrice,
    decimal StopLoss,
    decimal Quantity,
    decimal RemainingQuantity,
    decimal RealizedPnL,
    IReadOnlyList<decimal> TakeProfitPrices,
    DateTime OpenedAtUtc,
    DateTime? ClosedAtUtc)
{
    public static PositionDto From(Position p) => new(
        p.Id, p.Symbol, p.Side.ToString(), p.Status.ToString(),
        p.EntryPrice, p.StopLoss, p.Quantity, p.RemainingQuantity,
        p.RealizedPnL, p.TakeProfitPrices, p.OpenedAtUtc, p.ClosedAtUtc);
}

public sealed record FillDto(
    Guid Id,
    Guid PositionId,
    string Symbol,
    string Side,
    bool IsEntry,
    decimal Price,
    decimal Quantity,
    decimal Fee,
    int? PartialTpIndex,
    DateTime TimestampUtc)
{
    public static FillDto From(Fill f) => new(
        f.Id, f.PositionId, f.Symbol, f.Side.ToString(),
        f.IsEntry, f.Price, f.Quantity, f.Fee,
        f.PartialTpIndex, f.TimestampUtc);
}

public sealed record HealthDto(
    string Status,
    string TradingMode,
    bool MarketDataConnected,
    decimal LastPrice,
    string Symbol);

public sealed record ConfigDto(
    string Mode,
    string Symbol,
    int Leverage,
    decimal RiskPerTradePercent,
    int MaxOpenPositions);

public static class ConfigDtoMapper
{
    public static ConfigDto From(TradingOptions o) => new(
        o.Mode.ToString(), o.Symbol, o.Leverage, o.RiskPerTradePercent, o.MaxOpenPositions);
}

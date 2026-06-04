using ShineScalper.Core.Models;

namespace ShineScalper.Core.Interfaces;

public interface ITradingEventPublisher
{
    Task PublishCandleAsync(Candle candle, CancellationToken ct = default);
    Task PublishLevelsAsync(string symbol, IReadOnlyList<PriceZone> levels, CancellationToken ct = default);
    Task PublishSignalAsync(TradeSignal signal, CancellationToken ct = default);
    Task PublishPositionAsync(Position position, CancellationToken ct = default);
    Task PublishTradeAsync(Fill fill, CancellationToken ct = default);
    Task PublishTickerAsync(string symbol, decimal price, CancellationToken ct = default);
}

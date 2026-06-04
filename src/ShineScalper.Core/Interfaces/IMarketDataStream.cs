using ShineScalper.Core.Models;

namespace ShineScalper.Core.Interfaces;

public interface IMarketDataStream
{
    event Action<Candle>? CandleUpdated;
    event Action<string, decimal>? TickerUpdated;
    bool IsConnected { get; }
}

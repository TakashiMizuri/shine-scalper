using ShineScalper.Core.Models;

namespace ShineScalper.Core.Interfaces;

public interface ISignalEngine
{
    void OnTicker(string symbol, decimal lastPrice, DateTime utcNow);
    event Action<TradeSignal>? SignalGenerated;
}

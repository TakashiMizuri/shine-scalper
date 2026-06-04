using ShineScalper.Core.Models;

namespace ShineScalper.Core.Services;

public sealed class TradingStateCache
{
    private readonly object _lock = new();
    private IReadOnlyList<PriceZone> _levels = [];
    private decimal _lastPrice;
    private bool _marketDataConnected;

    public decimal LastPrice
    {
        get { lock (_lock) return _lastPrice; }
    }

    public bool MarketDataConnected
    {
        get { lock (_lock) return _marketDataConnected; }
    }

    public void SetLastPrice(decimal price)
    {
        lock (_lock) _lastPrice = price;
    }

    public void SetMarketDataConnected(bool connected)
    {
        lock (_lock) _marketDataConnected = connected;
    }

    public IReadOnlyList<PriceZone> GetLevels()
    {
        lock (_lock) return _levels;
    }

    public void SetLevels(IReadOnlyList<PriceZone> levels)
    {
        lock (_lock) _levels = levels;
    }
}

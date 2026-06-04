using ShineScalper.Core.Models;

namespace ShineScalper.Core.Interfaces;

public interface IExchangeClient
{
    Task<IReadOnlyList<Candle>> GetKlinesAsync(string symbol, string interval, int limit, CancellationToken ct = default);
}

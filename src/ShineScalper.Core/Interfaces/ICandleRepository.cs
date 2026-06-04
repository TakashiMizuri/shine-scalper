using ShineScalper.Core.Models;

namespace ShineScalper.Core.Interfaces;

public interface ICandleRepository
{
    Task UpsertAsync(Candle candle, CancellationToken ct = default);
    Task<IReadOnlyList<Candle>> GetAsync(string symbol, string interval, DateTime? sinceUtc, int limit, CancellationToken ct = default);
}

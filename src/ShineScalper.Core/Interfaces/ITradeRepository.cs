using ShineScalper.Core.Enums;
using ShineScalper.Core.Models;

namespace ShineScalper.Core.Interfaces;

public interface ITradeRepository
{
    Task SaveSignalAsync(TradeSignal signal, CancellationToken ct = default);
    Task SavePositionAsync(Position position, CancellationToken ct = default);
    Task UpdatePositionAsync(Position position, CancellationToken ct = default);
    Task SaveFillAsync(Fill fill, CancellationToken ct = default);
    Task<IReadOnlyList<Position>> GetOpenPositionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Position>> GetRecentPositionsAsync(int limit, CancellationToken ct = default);
    Task<IReadOnlyList<Fill>> GetFillsAsync(int limit, CancellationToken ct = default);
    Task SaveLevelSnapshotAsync(string symbol, IReadOnlyList<PriceZone> levels, CancellationToken ct = default);
    Task<decimal> GetEquityAsync(CancellationToken ct = default);
    Task SetEquityAsync(decimal equity, CancellationToken ct = default);
    Task SaveEquitySnapshotAsync(decimal equity, CancellationToken ct = default);
    Task<IReadOnlyList<(DateTime Time, decimal Equity)>> GetEquityHistoryAsync(int limit, CancellationToken ct = default);
    Task<decimal> GetDailyPnLAsync(DateTime utcDate, CancellationToken ct = default);
    Task AddDailyPnLAsync(decimal pnl, DateTime utcDate, CancellationToken ct = default);
    Task<int> CountOpenPositionsAsync(CancellationToken ct = default);
    Task<bool> HasOpenPositionForSymbolAsync(string symbol, CancellationToken ct = default);
}

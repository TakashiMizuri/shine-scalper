using ShineScalper.Core.Models;

namespace ShineScalper.Core.Interfaces;

public interface ILevelDetector
{
    IReadOnlyList<PriceZone> GetActiveLevels(string symbol);
    Task RefreshAsync(string symbol, CancellationToken ct = default);
}

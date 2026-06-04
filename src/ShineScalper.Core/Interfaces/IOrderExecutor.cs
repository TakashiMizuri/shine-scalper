using ShineScalper.Core.Models;

namespace ShineScalper.Core.Interfaces;

public interface IOrderExecutor
{
    Task<Fill> OpenPositionAsync(Position position, decimal marketPrice, CancellationToken ct = default);
    Task<Fill?> ClosePartialAsync(Position position, decimal quantity, decimal marketPrice, int partialTpIndex, CancellationToken ct = default);
    Task<Fill> CloseFullAsync(Position position, decimal marketPrice, string reason, CancellationToken ct = default);
}

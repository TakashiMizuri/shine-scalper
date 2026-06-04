using Microsoft.Extensions.Logging;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;

namespace ShineScalper.Engine.Execution;

public sealed class LiveOrderExecutor(ILogger<LiveOrderExecutor> logger) : IOrderExecutor
{
    public Task<Fill> OpenPositionAsync(Position position, decimal marketPrice, CancellationToken ct = default)
    {
        logger.LogWarning("Live trading is not implemented in v0.1");
        throw new NotSupportedException("Live trading is not implemented in v0.1. Use Paper mode.");
    }

    public Task<Fill?> ClosePartialAsync(Position position, decimal quantity, decimal marketPrice, int partialTpIndex, CancellationToken ct = default) =>
        throw new NotSupportedException("Live trading is not implemented in v0.1.");

    public Task<Fill> CloseFullAsync(Position position, decimal marketPrice, string reason, CancellationToken ct = default) =>
        throw new NotSupportedException("Live trading is not implemented in v0.1.");
}

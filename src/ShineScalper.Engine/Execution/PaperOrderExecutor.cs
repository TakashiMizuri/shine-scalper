using Microsoft.Extensions.Options;
using ShineScalper.Core.Enums;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;
using ShineScalper.Core.Options;

namespace ShineScalper.Engine.Execution;

public sealed class PaperOrderExecutor : IOrderExecutor
{
    private readonly ITradeRepository _trades;
    private readonly ExecutionOptions _execution;

    public PaperOrderExecutor(ITradeRepository trades, IOptions<ExecutionOptions> execution)
    {
        _trades = trades;
        _execution = execution.Value;
    }

    public async Task<Fill> OpenPositionAsync(Position position, decimal marketPrice, CancellationToken ct = default)
    {
        var fillPrice = ApplySlippage(marketPrice, position.Side, isEntry: true);
        var fee = CalcFee(fillPrice * position.Quantity);

        var fill = new Fill
        {
            PositionId = position.Id,
            Symbol = position.Symbol,
            Side = position.Side,
            IsEntry = true,
            Price = fillPrice,
            Quantity = position.Quantity,
            Fee = fee,
            TimestampUtc = DateTime.UtcNow
        };

        await _trades.SaveFillAsync(fill, ct);
        await UpdateEquityAsync(-fee, ct);
        return fill;
    }

    public async Task<Fill?> ClosePartialAsync(Position position, decimal quantity, decimal marketPrice, int partialTpIndex, CancellationToken ct = default)
    {
        if (quantity <= 0 || position.RemainingQuantity <= 0)
            return null;

        var qty = Math.Min(quantity, position.RemainingQuantity);
        var fillPrice = ApplySlippage(marketPrice, position.Side, isEntry: false);
        var fee = CalcFee(fillPrice * qty);
        var pnl = CalcPnL(position, fillPrice, qty) - fee;

        position.RemainingQuantity -= qty;
        position.RealizedPnL += pnl;
        position.PartialTpIndex = partialTpIndex;

        var fill = new Fill
        {
            PositionId = position.Id,
            Symbol = position.Symbol,
            Side = position.Side,
            IsEntry = false,
            Price = fillPrice,
            Quantity = qty,
            Fee = fee,
            PartialTpIndex = partialTpIndex,
            TimestampUtc = DateTime.UtcNow
        };

        await _trades.SaveFillAsync(fill, ct);
        await UpdateEquityAsync(pnl, ct);
        return fill;
    }

    public async Task<Fill> CloseFullAsync(Position position, decimal marketPrice, string reason, CancellationToken ct = default)
    {
        var fill = await ClosePartialAsync(position, position.RemainingQuantity, marketPrice, position.PartialTpIndex, ct)
                   ?? throw new InvalidOperationException("Nothing to close");

        position.Status = PositionStatus.Closed;
        position.ClosedAtUtc = DateTime.UtcNow;
        position.RemainingQuantity = 0;
        await _trades.UpdatePositionAsync(position, ct);
        return fill;
    }

    private decimal ApplySlippage(decimal price, TradeSide side, bool isEntry)
    {
        var bps = _execution.PaperSlippageBps / 10000m;
        return (side, isEntry) switch
        {
            (TradeSide.Long, true) => price * (1 + bps),
            (TradeSide.Long, false) => price * (1 - bps),
            (TradeSide.Short, true) => price * (1 - bps),
            (TradeSide.Short, false) => price * (1 + bps),
            _ => price
        };
    }

    private decimal CalcFee(decimal notional) => notional * _execution.FeeTakerBps / 10000m;

    private static decimal CalcPnL(Position position, decimal exitPrice, decimal qty) =>
        position.Side switch
        {
            TradeSide.Long => (exitPrice - position.EntryPrice) * qty,
            TradeSide.Short => (position.EntryPrice - exitPrice) * qty,
            _ => 0
        };

    private async Task UpdateEquityAsync(decimal pnl, CancellationToken ct)
    {
        var equity = await _trades.GetEquityAsync(ct) + pnl;
        await _trades.SetEquityAsync(equity, ct);
        await _trades.SaveEquitySnapshotAsync(equity, ct);
    }
}

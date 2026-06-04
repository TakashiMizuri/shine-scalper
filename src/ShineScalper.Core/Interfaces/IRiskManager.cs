using ShineScalper.Core.Models;

namespace ShineScalper.Core.Interfaces;

public interface IRiskManager
{
    decimal GetEquity();
    bool CanOpenPosition(string symbol, out string? rejectReason);
    bool TryBuildPosition(TradeSignal signal, decimal entryPrice, out Position? position, out string? rejectReason);
    void RecordDailyPnL(decimal pnl);
}

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShineScalper.Core.Enums;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;
using ShineScalper.Infrastructure.Data;
using ShineScalper.Infrastructure.Entities;

namespace ShineScalper.Infrastructure.Repositories;

public sealed class TradeRepository(TradingDbContext db) : ITradeRepository
{
    public async Task SaveSignalAsync(TradeSignal signal, CancellationToken ct = default)
    {
        db.Signals.Add(new SignalEntity
        {
            Id = signal.Id,
            Symbol = signal.Symbol,
            Side = (int)signal.Side,
            LevelId = signal.LevelId,
            Status = (int)signal.Status,
            Reason = signal.Reason,
            RejectReason = signal.RejectReason,
            TriggerPrice = signal.TriggerPrice,
            TimestampUtc = signal.TimestampUtc
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task SavePositionAsync(Position position, CancellationToken ct = default)
    {
        db.Positions.Add(MapPosition(position));
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdatePositionAsync(Position position, CancellationToken ct = default)
    {
        var entity = await db.Positions.FindAsync([position.Id], ct);
        if (entity is null) return;

        entity.Status = (int)position.Status;
        entity.RemainingQuantity = position.RemainingQuantity;
        entity.ClosedAtUtc = position.ClosedAtUtc;
        entity.RealizedPnL = position.RealizedPnL;
        entity.PartialTpIndex = position.PartialTpIndex;
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveFillAsync(Fill fill, CancellationToken ct = default)
    {
        db.Fills.Add(new FillEntity
        {
            Id = fill.Id,
            PositionId = fill.PositionId,
            Symbol = fill.Symbol,
            Side = (int)fill.Side,
            IsEntry = fill.IsEntry,
            Price = fill.Price,
            Quantity = fill.Quantity,
            Fee = fill.Fee,
            PartialTpIndex = fill.PartialTpIndex,
            TimestampUtc = fill.TimestampUtc
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Position>> GetOpenPositionsAsync(CancellationToken ct = default)
    {
        var entities = await db.Positions.AsNoTracking()
            .Where(p => p.Status == (int)PositionStatus.Open)
            .ToListAsync(ct);
        return entities.Select(MapPositionEntity).ToList();
    }

    public async Task<IReadOnlyList<Position>> GetRecentPositionsAsync(int limit, CancellationToken ct = default)
    {
        var entities = await db.Positions.AsNoTracking()
            .OrderByDescending(p => p.OpenedAtUtc)
            .Take(limit)
            .ToListAsync(ct);
        return entities.Select(MapPositionEntity).ToList();
    }

    public async Task<IReadOnlyList<Fill>> GetFillsAsync(int limit, CancellationToken ct = default)
    {
        var entities = await db.Fills.AsNoTracking()
            .OrderByDescending(f => f.TimestampUtc)
            .Take(limit)
            .ToListAsync(ct);

        return entities.Select(e => new Fill
        {
            Id = e.Id,
            PositionId = e.PositionId,
            Symbol = e.Symbol,
            Side = (TradeSide)e.Side,
            IsEntry = e.IsEntry,
            Price = e.Price,
            Quantity = e.Quantity,
            Fee = e.Fee,
            PartialTpIndex = e.PartialTpIndex,
            TimestampUtc = e.TimestampUtc
        }).ToList();
    }

    public async Task SaveLevelSnapshotAsync(string symbol, IReadOnlyList<PriceZone> levels, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        foreach (var level in levels)
        {
            db.LevelSnapshots.Add(new LevelSnapshotEntity
            {
                Symbol = symbol,
                LevelId = level.Id,
                Center = level.Center,
                LowerBound = level.LowerBound,
                UpperBound = level.UpperBound,
                LevelType = (int)level.Type,
                Touches = level.Touches,
                LastTouchUtc = level.LastTouchUtc,
                Invalidated = level.Invalidated,
                DetectedAtUtc = level.DetectedAtUtc,
                SnapshotAtUtc = now
            });
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task<decimal> GetEquityAsync(CancellationToken ct = default)
    {
        var state = await db.BotStates.FindAsync([1], ct);
        return state?.Equity ?? 0;
    }

    public async Task SetEquityAsync(decimal equity, CancellationToken ct = default)
    {
        var state = await db.BotStates.FindAsync([1], ct);
        if (state is null)
        {
            db.BotStates.Add(new BotStateEntity { Id = 1, Equity = equity, UpdatedAtUtc = DateTime.UtcNow });
        }
        else
        {
            state.Equity = equity;
            state.UpdatedAtUtc = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveEquitySnapshotAsync(decimal equity, CancellationToken ct = default)
    {
        db.EquitySnapshots.Add(new EquitySnapshotEntity
        {
            Equity = equity,
            TimestampUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<(DateTime Time, decimal Equity)>> GetEquityHistoryAsync(int limit, CancellationToken ct = default)
    {
        var rows = await db.EquitySnapshots.AsNoTracking()
            .OrderByDescending(e => e.TimestampUtc)
            .Take(limit)
            .ToListAsync(ct);

        return rows
            .OrderBy(r => r.TimestampUtc)
            .Select(r => (r.TimestampUtc, r.Equity))
            .ToList();
    }

    public async Task<decimal> GetDailyPnLAsync(DateTime utcDate, CancellationToken ct = default)
    {
        var date = DateOnly.FromDateTime(utcDate);
        var row = await db.DailyPnLs.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Date == date, ct);
        return row?.PnL ?? 0;
    }

    public async Task AddDailyPnLAsync(decimal pnl, DateTime utcDate, CancellationToken ct = default)
    {
        var date = DateOnly.FromDateTime(utcDate);
        var row = await db.DailyPnLs.FirstOrDefaultAsync(d => d.Date == date, ct);
        if (row is null)
        {
            db.DailyPnLs.Add(new DailyPnLEntity { Date = date, PnL = pnl });
        }
        else
        {
            row.PnL += pnl;
        }
        await db.SaveChangesAsync(ct);
    }

    public Task<int> CountOpenPositionsAsync(CancellationToken ct = default) =>
        db.Positions.CountAsync(p => p.Status == (int)PositionStatus.Open, ct);

    public Task<bool> HasOpenPositionForSymbolAsync(string symbol, CancellationToken ct = default) =>
        db.Positions.AnyAsync(p => p.Status == (int)PositionStatus.Open && p.Symbol == symbol, ct);

    private static PositionEntity MapPosition(Position p) => new()
    {
        Id = p.Id,
        Symbol = p.Symbol,
        Side = (int)p.Side,
        Status = (int)p.Status,
        EntryPrice = p.EntryPrice,
        StopLoss = p.StopLoss,
        Quantity = p.Quantity,
        RemainingQuantity = p.RemainingQuantity,
        StopLossMode = (int)p.StopLossMode,
        LevelId = p.LevelId,
        OpenedAtUtc = p.OpenedAtUtc,
        ClosedAtUtc = p.ClosedAtUtc,
        RealizedPnL = p.RealizedPnL,
        TakeProfitPricesJson = JsonSerializer.Serialize(p.TakeProfitPrices),
        PartialTpIndex = p.PartialTpIndex
    };

    private static Position MapPositionEntity(PositionEntity e) => new()
    {
        Id = e.Id,
        Symbol = e.Symbol,
        Side = (TradeSide)e.Side,
        Status = (PositionStatus)e.Status,
        EntryPrice = e.EntryPrice,
        StopLoss = e.StopLoss,
        Quantity = e.Quantity,
        RemainingQuantity = e.RemainingQuantity,
        StopLossMode = (StopLossMode)e.StopLossMode,
        LevelId = e.LevelId,
        OpenedAtUtc = e.OpenedAtUtc,
        ClosedAtUtc = e.ClosedAtUtc,
        RealizedPnL = e.RealizedPnL,
        TakeProfitPrices = JsonSerializer.Deserialize<List<decimal>>(e.TakeProfitPricesJson) ?? [],
        PartialTpIndex = e.PartialTpIndex
    };
}

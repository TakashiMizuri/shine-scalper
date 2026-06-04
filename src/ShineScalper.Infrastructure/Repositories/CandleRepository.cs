using Microsoft.EntityFrameworkCore;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Models;
using ShineScalper.Infrastructure.Data;
using ShineScalper.Infrastructure.Entities;

namespace ShineScalper.Infrastructure.Repositories;

public sealed class CandleRepository(TradingDbContext db) : ICandleRepository
{
    public async Task UpsertAsync(Candle candle, CancellationToken ct = default)
    {
        var existing = await db.Candles
            .FirstOrDefaultAsync(c =>
                c.Symbol == candle.Symbol &&
                c.Interval == candle.Interval &&
                c.OpenTimeMs == candle.OpenTimeMs, ct);

        if (existing is null)
        {
            db.Candles.Add(new CandleEntity
            {
                Symbol = candle.Symbol,
                Interval = candle.Interval,
                OpenTimeMs = candle.OpenTimeMs,
                Open = candle.Open,
                High = candle.High,
                Low = candle.Low,
                Close = candle.Close,
                Volume = candle.Volume,
                IsClosed = candle.IsClosed
            });
        }
        else
        {
            existing.Open = candle.Open;
            existing.High = candle.High;
            existing.Low = candle.Low;
            existing.Close = candle.Close;
            existing.Volume = candle.Volume;
            existing.IsClosed = candle.IsClosed;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Candle>> GetAsync(string symbol, string interval, DateTime? sinceUtc, int limit, CancellationToken ct = default)
    {
        var query = db.Candles.AsNoTracking()
            .Where(c => c.Symbol == symbol && c.Interval == interval);

        if (sinceUtc.HasValue)
        {
            var sinceMs = new DateTimeOffset(sinceUtc.Value).ToUnixTimeMilliseconds();
            query = query.Where(c => c.OpenTimeMs >= sinceMs);
        }

        var entities = await query
            .OrderByDescending(c => c.OpenTimeMs)
            .Take(limit)
            .ToListAsync(ct);

        return entities
            .OrderBy(c => c.OpenTimeMs)
            .Select(Map)
            .ToList();
    }

    private static Candle Map(CandleEntity e) => new()
    {
        Symbol = e.Symbol,
        Interval = e.Interval,
        OpenTimeMs = e.OpenTimeMs,
        Open = e.Open,
        High = e.High,
        Low = e.Low,
        Close = e.Close,
        Volume = e.Volume,
        IsClosed = e.IsClosed
    };
}

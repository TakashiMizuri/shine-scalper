using Microsoft.EntityFrameworkCore;
using ShineScalper.Infrastructure.Entities;

namespace ShineScalper.Infrastructure.Data;

public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
{
    public DbSet<CandleEntity> Candles => Set<CandleEntity>();
    public DbSet<LevelSnapshotEntity> LevelSnapshots => Set<LevelSnapshotEntity>();
    public DbSet<SignalEntity> Signals => Set<SignalEntity>();
    public DbSet<PositionEntity> Positions => Set<PositionEntity>();
    public DbSet<FillEntity> Fills => Set<FillEntity>();
    public DbSet<EquitySnapshotEntity> EquitySnapshots => Set<EquitySnapshotEntity>();
    public DbSet<BotStateEntity> BotStates => Set<BotStateEntity>();
    public DbSet<DailyPnLEntity> DailyPnLs => Set<DailyPnLEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CandleEntity>(e =>
        {
            e.HasIndex(x => new { x.Symbol, x.Interval, x.OpenTimeMs }).IsUnique();
        });

        modelBuilder.Entity<BotStateEntity>(e =>
        {
            e.HasKey(x => x.Id);
        });
    }
}

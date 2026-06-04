using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShineScalper.Infrastructure.Data;

#nullable disable

namespace ShineScalper.Infrastructure.Migrations;

[DbContext(typeof(TradingDbContext))]
partial class TradingDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

        modelBuilder.Entity("ShineScalper.Infrastructure.Entities.BotStateEntity", b =>
        {
            b.Property<int>("Id").HasColumnType("INTEGER");
            b.Property<decimal>("Equity").HasColumnType("TEXT");
            b.Property<DateTime>("UpdatedAtUtc").HasColumnType("TEXT");
            b.HasKey("Id");
            b.ToTable("BotStates");
        });

        modelBuilder.Entity("ShineScalper.Infrastructure.Entities.CandleEntity", b =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<decimal>("Close").HasColumnType("TEXT");
            b.Property<decimal>("High").HasColumnType("TEXT");
            b.Property<string>("Interval").IsRequired().HasColumnType("TEXT");
            b.Property<bool>("IsClosed").HasColumnType("INTEGER");
            b.Property<decimal>("Low").HasColumnType("TEXT");
            b.Property<decimal>("Open").HasColumnType("TEXT");
            b.Property<long>("OpenTimeMs").HasColumnType("INTEGER");
            b.Property<string>("Symbol").IsRequired().HasColumnType("TEXT");
            b.Property<decimal>("Volume").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("Symbol", "Interval", "OpenTimeMs").IsUnique();
            b.ToTable("Candles");
        });
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShineScalper.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BotStates",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false),
                Equity = table.Column<decimal>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_BotStates", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Candles",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Symbol = table.Column<string>(type: "TEXT", nullable: false),
                Interval = table.Column<string>(type: "TEXT", nullable: false),
                OpenTimeMs = table.Column<long>(type: "INTEGER", nullable: false),
                Open = table.Column<decimal>(type: "TEXT", nullable: false),
                High = table.Column<decimal>(type: "TEXT", nullable: false),
                Low = table.Column<decimal>(type: "TEXT", nullable: false),
                Close = table.Column<decimal>(type: "TEXT", nullable: false),
                Volume = table.Column<decimal>(type: "TEXT", nullable: false),
                IsClosed = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Candles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "DailyPnLs",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                PnL = table.Column<decimal>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_DailyPnLs", x => x.Id));

        migrationBuilder.CreateTable(
            name: "EquitySnapshots",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Equity = table.Column<decimal>(type: "TEXT", nullable: false),
                TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_EquitySnapshots", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Fills",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                PositionId = table.Column<Guid>(type: "TEXT", nullable: false),
                Symbol = table.Column<string>(type: "TEXT", nullable: false),
                Side = table.Column<int>(type: "INTEGER", nullable: false),
                IsEntry = table.Column<bool>(type: "INTEGER", nullable: false),
                Price = table.Column<decimal>(type: "TEXT", nullable: false),
                Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                Fee = table.Column<decimal>(type: "TEXT", nullable: false),
                PartialTpIndex = table.Column<int>(type: "INTEGER", nullable: true),
                TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Fills", x => x.Id));

        migrationBuilder.CreateTable(
            name: "LevelSnapshots",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Symbol = table.Column<string>(type: "TEXT", nullable: false),
                LevelId = table.Column<string>(type: "TEXT", nullable: false),
                Center = table.Column<decimal>(type: "TEXT", nullable: false),
                LowerBound = table.Column<decimal>(type: "TEXT", nullable: false),
                UpperBound = table.Column<decimal>(type: "TEXT", nullable: false),
                LevelType = table.Column<int>(type: "INTEGER", nullable: false),
                Touches = table.Column<int>(type: "INTEGER", nullable: false),
                LastTouchUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                Invalidated = table.Column<bool>(type: "INTEGER", nullable: false),
                DetectedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                SnapshotAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_LevelSnapshots", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Positions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Symbol = table.Column<string>(type: "TEXT", nullable: false),
                Side = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                EntryPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                StopLoss = table.Column<decimal>(type: "TEXT", nullable: false),
                Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                RemainingQuantity = table.Column<decimal>(type: "TEXT", nullable: false),
                StopLossMode = table.Column<int>(type: "INTEGER", nullable: false),
                LevelId = table.Column<string>(type: "TEXT", nullable: false),
                OpenedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                ClosedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                RealizedPnL = table.Column<decimal>(type: "TEXT", nullable: false),
                TakeProfitPricesJson = table.Column<string>(type: "TEXT", nullable: false),
                PartialTpIndex = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Positions", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Signals",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Symbol = table.Column<string>(type: "TEXT", nullable: false),
                Side = table.Column<int>(type: "INTEGER", nullable: false),
                LevelId = table.Column<string>(type: "TEXT", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                Reason = table.Column<string>(type: "TEXT", nullable: false),
                RejectReason = table.Column<string>(type: "TEXT", nullable: true),
                TriggerPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Signals", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Candles_Symbol_Interval_OpenTimeMs",
            table: "Candles",
            columns: new[] { "Symbol", "Interval", "OpenTimeMs" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "BotStates");
        migrationBuilder.DropTable(name: "Candles");
        migrationBuilder.DropTable(name: "DailyPnLs");
        migrationBuilder.DropTable(name: "EquitySnapshots");
        migrationBuilder.DropTable(name: "Fills");
        migrationBuilder.DropTable(name: "LevelSnapshots");
        migrationBuilder.DropTable(name: "Positions");
        migrationBuilder.DropTable(name: "Signals");
    }
}

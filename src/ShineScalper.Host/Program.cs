using Microsoft.EntityFrameworkCore;
using ShineScalper.Api;
using ShineScalper.Api.Hubs;
using ShineScalper.Bybit;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Options;
using ShineScalper.Engine;
using ShineScalper.Infrastructure;
using ShineScalper.Infrastructure.Data;
using ShineScalper.Infrastructure.Entities;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TradingDb")
    ?? "Data Source=shine-scalper.db";

builder.Services.AddInfrastructure(connectionString);
builder.Services.Configure<TradingOptions>(builder.Configuration.GetSection(TradingOptions.SectionName));
builder.Services.Configure<LevelOptions>(builder.Configuration.GetSection(LevelOptions.SectionName));
builder.Services.Configure<SignalOptions>(builder.Configuration.GetSection(SignalOptions.SectionName));
builder.Services.Configure<RiskOptions>(builder.Configuration.GetSection(RiskOptions.SectionName));
builder.Services.Configure<ExecutionOptions>(builder.Configuration.GetSection(ExecutionOptions.SectionName));
builder.Services.Configure<BybitOptions>(builder.Configuration.GetSection(BybitOptions.SectionName));
builder.Services.AddApiServices();
builder.Services.AddEngine();
builder.Services.AddBybit();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();
await SeedBotStateAsync(app.Services, builder.Configuration);

app.UseCors();
app.MapControllers();
app.MapHub<TradingHub>(TradingHub.HubPath);
app.MapGet("/", () => Results.Redirect("/api/health"));

app.Run();

static async Task SeedBotStateAsync(IServiceProvider sp, IConfiguration config)
{
    using var scope = sp.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
    var trades = scope.ServiceProvider.GetRequiredService<ITradeRepository>();
    var initial = config.GetSection(TradingOptions.SectionName).Get<TradingOptions>()?.InitialPaperEquity ?? 10_000m;

    if (!await db.BotStates.AnyAsync())
    {
        db.BotStates.Add(new BotStateEntity
        {
            Id = 1,
            Equity = initial,
            UpdatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        await trades.SaveEquitySnapshotAsync(initial);
    }
}

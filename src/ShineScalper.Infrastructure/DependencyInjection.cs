using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShineScalper.Core.Interfaces;
using ShineScalper.Infrastructure.Data;
using ShineScalper.Infrastructure.Repositories;

namespace ShineScalper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TradingDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<ICandleRepository, CandleRepository>();
        services.AddScoped<ITradeRepository, TradeRepository>();
        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}

using Microsoft.Extensions.DependencyInjection;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Options;

namespace ShineScalper.Bybit;

public static class DependencyInjection
{
    public static IServiceCollection AddBybit(this IServiceCollection services)
    {
        services.AddHttpClient<IExchangeClient, BybitRestClient>();
        services.AddSingleton<BybitMarketDataService>();
        services.AddHostedService(sp => sp.GetRequiredService<BybitMarketDataService>());
        services.AddSingleton<IMarketDataStream>(sp => sp.GetRequiredService<BybitMarketDataService>());
        return services;
    }

    public static IServiceCollection ConfigureBybitOptions(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration config)
    {
        services.Configure<BybitOptions>(config.GetSection(BybitOptions.SectionName));
        return services;
    }
}

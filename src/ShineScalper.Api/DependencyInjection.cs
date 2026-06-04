using Microsoft.Extensions.DependencyInjection;
using ShineScalper.Api.Hubs;
using ShineScalper.Api.Services;
using ShineScalper.Core.Interfaces;

namespace ShineScalper.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<ITradingEventPublisher, SignalRTradingEventPublisher>();
        services.AddControllers();
        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using ShineScalper.Core.Interfaces;
using ShineScalper.Core.Options;
using ShineScalper.Core.Services;
using ShineScalper.Engine.Execution;
using ShineScalper.Engine.Levels;
using ShineScalper.Engine.Signals;

namespace ShineScalper.Engine;

public static class DependencyInjection
{
    public static IServiceCollection AddEngine(this IServiceCollection services)
    {
        services.AddSingleton<TradingStateCache>();
        services.AddSingleton<LevelDetectorService>();
        services.AddHostedService(sp => sp.GetRequiredService<LevelDetectorService>());
        services.AddSingleton<ILevelDetector>(sp => sp.GetRequiredService<LevelDetectorService>());
        services.AddSingleton<ISignalEngine, BreakoutSignalEngine>();
        services.AddScoped<IRiskManager, Risk.RiskManager>();
        services.AddScoped<PaperOrderExecutor>();
        services.AddScoped<LiveOrderExecutor>();
        services.AddHostedService<TradingOrchestrator>();
        return services;
    }

}

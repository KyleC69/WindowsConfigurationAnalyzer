using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Context;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Readers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWcaCore(this IServiceCollection services)
    {
        // Core infra
        services.AddSingleton<ITimeProvider, SystemTimeProvider>();
        services.AddSingleton<ActionLogger>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("WCA.ActionLogger");
            return new ActionLogger(logger);
        });

        // Readers
        services.AddSingleton<IEnvReader, EnvironmentReader>();
        services.AddSingleton<IRegistryReader, RegistryReader>();
        services.AddSingleton<ICimReader, CimReader>();
        services.AddSingleton<IEventLogReader, EventLogReader>();
        services.AddSingleton<IFirewallReader, FirewallReader>();

        // Context
        services.AddScoped<IAnalyzerContext>(sp =>
        new AnalyzerContext(
        sp.GetRequiredService<ILoggerFactory>().CreateLogger("WCA"),
        sp.GetRequiredService<ITimeProvider>(),
        sp.GetRequiredService<ActionLogger>(),
        sp.GetRequiredService<IRegistryReader>(),
        sp.GetRequiredService<ICimReader>(),
        sp.GetRequiredService<IEventLogReader>(),
        sp.GetRequiredService<IFirewallReader>(),
        sp.GetRequiredService<IEnvReader>()
        ));

        return services;
    }
}

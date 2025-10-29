using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsConfigurationAnalyzer.Contracts;
using WindowsConfigurationAnalyzer.Infrastructure;
using WindowsConfigurationAnalyzer.Readers;

namespace WindowsConfigurationAnalyzer;

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
 new WindowsConfigurationAnalyzer.Context.AnalyzerContext(
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsConfigurationAnalyzer.Contracts;
using WindowsConfigurationAnalyzer.Infrastructure;

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

 // Context factory (basic)
 services.AddScoped<IAnalyzerContext, DefaultAnalyzerContext>();

 return services;
 }
}

internal sealed class DefaultAnalyzerContext : IAnalyzerContext
{
 public DefaultAnalyzerContext(ILoggerFactory loggerFactory, ITimeProvider time, ActionLogger actionLogger)
 {
 Logger = loggerFactory.CreateLogger("WCA");
 Time = time;
 ActionLogger = actionLogger;
 }

 public ILogger Logger { get; }
 public ITimeProvider Time { get; }
 public ActionLogger ActionLogger { get; }

 public IRegistryReader? Registry => null;
 public ICimReader? Cim => null;
 public IEventLogReader? EventLog => null;
 public IFirewallReader? Firewall => null;
 public IEnvReader? Environment => null;
}

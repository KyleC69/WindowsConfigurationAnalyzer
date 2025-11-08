// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  ServiceCollectionExtensions.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Drivers;
using KC.WindowsConfigurationAnalyzer.Analyzer.Areas.EventLog;
using KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Hardware;
using KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Network;
using KC.WindowsConfigurationAnalyzer.Analyzer.Areas.OS;
using KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Performance;
using KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Policy;
using KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Security;
using KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Software;
using KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Startup;
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
        /*      services.AddSingleton<IEventProvider, EventProviderStub>(); // stub until manifest binding
              services.AddSingleton<ActionLogger>(sp =>
              {
                  var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("WCA.ActionLogger");
                  var provider = sp.GetRequiredService<IEventProvider>();
                  return new ActionLogger(logger, null, provider);
              });
      */
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

        // Default module registrations (host can add/override as needed)
        services.AddSingleton<IAnalyzerModule, OSAnalyzer>();
        services.AddSingleton<IAnalyzerModule, HardwareAnalyzer>();
        services.AddSingleton<IAnalyzerModule, NetworkAnalyzer>();
        services.AddSingleton<IAnalyzerModule, SecurityAnalyzer>();
        services.AddSingleton<IAnalyzerModule, SoftwareAnalyzer>();
        services.AddSingleton<IAnalyzerModule, PerformanceAnalyzer>();
        services.AddSingleton<IAnalyzerModule, PolicyAnalyzer>();
        services.AddSingleton<IAnalyzerModule, StartupAnalyzer>();
        services.AddSingleton<IAnalyzerModule, EventLogAnalyzer>();
        services.AddSingleton<IAnalyzerModule, DriversAnalyzer>();

        return services;
    }
}
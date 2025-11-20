// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  ServiceCollectionExtensions.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Drivers;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.EventLog;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Hardware;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Network;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.OS;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Performance;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Policy;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Security;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Software;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Startup;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;

using Microsoft.Extensions.DependencyInjection;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.DependencyInjection;


public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddWcaCore(this IServiceCollection services)
    {
        // Readers
        services.AddSingleton<IEnvReader, EnvironmentReader>();
        services.AddSingleton<IRegistryReader, RegistryReader>();
        services.AddSingleton<ICimReader, CimReader>();
        services.AddSingleton<IEventLogReader, EventLogReader>();
        services.AddSingleton<IFirewallReader, FirewallReader>();


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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsConfigurationAnalyzer; // for AddWcaCore
using WindowsConfigurationAnalyzer.Engine;
using WindowsConfigurationAnalyzer.Contracts;

namespace Analyzer;

public static class Startup
{
 public static IServiceProvider BuildServices()
 {
 var services = new ServiceCollection();
 services.AddLogging(b => b.AddDebug().AddConsole());
 services.AddWcaCore();
 services.AddSingleton<IAnalyzerModule, WindowsConfigurationAnalyzer.OS.OSAnalyzer>();
 services.AddSingleton<IAnalyzerModule, WindowsConfigurationAnalyzer.Hardware.HardwareAnalyzer>();
 services.AddSingleton<IAnalyzerModule, WindowsConfigurationAnalyzer.Network.NetworkAnalyzer>();
 services.AddSingleton<IAnalyzerModule, WindowsConfigurationAnalyzer.Security.SecurityAnalyzer>();
 services.AddSingleton<IAnalyzerModule, WindowsConfigurationAnalyzer.Software.SoftwareAnalyzer>();
 return services.BuildServiceProvider();
 }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsConfigurationAnalyzer; // for AddWcaCore extension
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
 return services.BuildServiceProvider();
 }
}

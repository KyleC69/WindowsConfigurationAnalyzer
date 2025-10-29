using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WindowsConfigurationAnalyzer.Engine;
using WindowsConfigurationAnalyzer.Contracts;

namespace Analyzer;

internal static class Program
{
 [STAThread]
 private static async Task Main(string[] args)
 {
 var services = Startup.BuildServices();
 var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("WCA.Engine");
 var engine = new AnalyzerEngine(logger);
 var context = services.GetRequiredService<IAnalyzerContext>();

 // Register all modules from DI
 foreach (var module in services.GetServices<IAnalyzerModule>())
 {
 engine.AddModule(module);
 }

 var result = await engine.RunAllAsync(context);
 logger.LogInformation("WCA run complete with {AreaCount} areas", result.Areas.Count);
 }
}

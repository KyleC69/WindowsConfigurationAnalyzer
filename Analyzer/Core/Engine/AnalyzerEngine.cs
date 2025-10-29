namespace WindowsConfigurationAnalyzer.Engine;

using Microsoft.Extensions.Logging;
using WindowsConfigurationAnalyzer.Contracts;
using WindowsConfigurationAnalyzer.Infrastructure;
using WindowsConfigurationAnalyzer.Models;

public sealed class AnalyzerEngine
{
 private readonly ILogger _logger;
 private readonly ITimeProvider _time;
 private readonly List<IAnalyzerModule> _modules = new();

 public AnalyzerEngine(ILogger logger, ITimeProvider? time = null)
 {
 _logger = logger;
 _time = time ?? new SystemTimeProvider();
 }

 public AnalyzerEngine AddModule(IAnalyzerModule module)
 {
 _modules.Add(module);
 return this;
 }

 public async Task<AnalyzerResult> RunAllAsync(IAnalyzerContext context, CancellationToken cancellationToken = default)
 {
 var actionLogger = context.ActionLogger;
 var areaResults = new List<AreaResult>();
 var globalFindings = new List<Finding>();

 using (_logger.BeginScope("WCA.RunAll"))
 {
 var tasks = _modules.Select(async m =>
 {
 try
 {
 actionLogger.Info(m.Area, "Start", $"Starting analyzer {m.Name}");
 var result = await m.AnalyzeAsync(context, cancellationToken).ConfigureAwait(false);
 actionLogger.Info(m.Area, "Complete", $"Completed analyzer {m.Name}");
 return result;
 }
 catch (OperationCanceledException)
 {
 actionLogger.Warn(m.Area, "Canceled", $"Analyzer {m.Name} canceled");
 return new AreaResult(m.Area, null, null, Array.Empty<Finding>(), Array.Empty<string>(), new[] { "Canceled" });
 }
 catch (Exception ex)
 {
 actionLogger.Error(m.Area, "Error", $"Analyzer {m.Name} failed", ex);
 return new AreaResult(m.Area, null, null, Array.Empty<Finding>(), Array.Empty<string>(), new[] { ex.Message });
 }
 });

 var results = await Task.WhenAll(tasks).ConfigureAwait(false);
 areaResults.AddRange(results);
 }

 var computer = Environment.MachineName;
 return new AnalyzerResult(
 computer,
 _time.UtcNow,
 areaResults,
 globalFindings,
 context.ActionLogger.Entries);
 }
}

using System.Reflection;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Diagnostics.Etw;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using Microsoft.Extensions.Logging;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Engine;

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

        // Emit ETW session start per taxonomy1001
        var sessionId = Guid.NewGuid().ToString("N");
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
        WcaEventSource.Log.SessionStart(sessionId, Environment.MachineName, version);

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

        // Emit ETW session stop per taxonomy1002
        var areasCount = areaResults.Count;
        var warnings = areaResults.Sum(r => r.Warnings.Count);
        var errors = areaResults.Sum(r => r.Errors.Count);
        var elapsed = 0d; // caller can compute externally if needed; placeholder
        WcaEventSource.Log.SessionStop(sessionId, areasCount, warnings, errors, elapsed);

        var computer = Environment.MachineName;
        return new AnalyzerResult(
        computer,
        _time.UtcNow,
        areaResults,
        globalFindings,
        context.ActionLogger.Entries);
    }
}

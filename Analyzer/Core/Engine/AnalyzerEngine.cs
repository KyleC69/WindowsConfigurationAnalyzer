// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  AnalyzerEngine.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Reflection;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using Microsoft.Extensions.Logging;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Engine;



public sealed class AnalyzerEngine
{
    private readonly IEventProvider? _eventProvider;
    private readonly ILogger _logger;
    private readonly List<IAnalyzerModule> _modules = [];
    private readonly ITimeProvider _time;





    public AnalyzerEngine(ILogger logger, ITimeProvider? time = null, IEventProvider? eventProvider = null)
    {
        _logger = logger;
        _time = time ?? new SystemTimeProvider();
        _eventProvider = eventProvider;
    }





    public AnalyzerEngine AddModule(IAnalyzerModule module)
    {
        _modules.Add(module);

        return this;
    }





    public async Task<AnalyzerResult> RunAllAsync(IAnalyzerContext context,
                                                  CancellationToken cancellationToken = default(CancellationToken))
    {
        var actionLogger = context.ActionLogger;
        List<AreaResult> areaResults = new();
        List<Finding> globalFindings = new();

        // Emit ETW session start per taxonomy1001
        var sessionId = Guid.NewGuid().ToString("N");
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
        try
        {
            _eventProvider?.EmitSessionStart(sessionId, Environment.MachineName, version);
        }
        catch
        {
        }

        using (_logger.BeginScope("WCA.RunAll"))
        {
            IEnumerable<Task<AreaResult>> tasks = _modules.Select(async m =>
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

                    return new AreaResult(m.Area, null, null, Array.Empty<Finding>(), Array.Empty<string>(),
                        new[] { "Canceled" });
                }
                catch (Exception ex)
                {
                    actionLogger.Error(m.Area, "Error", $"Analyzer {m.Name} failed", ex);

                    return new AreaResult(m.Area, null, null, Array.Empty<Finding>(), Array.Empty<string>(),
                        new[] { ex.Message });
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
        try
        {
            _eventProvider?.EmitSessionStop(sessionId, areasCount, warnings, errors, elapsed);
        }
        catch
        {
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
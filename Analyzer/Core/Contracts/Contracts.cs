using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using Microsoft.Extensions.Logging;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;

public interface IAnalyzerModule
{
    string Name
    {
        get;
    }
    string Area
    {
        get;
    }
    Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken);
}

public interface IAnalyzerContext
{
    ILogger Logger
    {
        get;
    }
    ITimeProvider Time
    {
        get;
    }
    ActionLogger ActionLogger
    {
        get;
    }

    IRegistryReader Registry
    {
        get;
    }
    ICimReader Cim
    {
        get;
    }
    IEventLogReader EventLog
    {
        get;
    }
    IFirewallReader Firewall
    {
        get;
    }
    IEnvReader Environment
    {
        get;
    }
}

public interface IExporter
{
    Task ExportAsync(AnalyzerResult result, string targetPath, CancellationToken cancellationToken);
}

public interface IAnomalyDetector
{
    IReadOnlyList<Finding> Detect(AnalyzerResult result);
}

public interface IRule
{
    string Id
    {
        get;
    }
    string Area
    {
        get;
    }
    Finding? Evaluate(AnalyzerResult result);
}

// Reader contracts
public interface IRegistryReader
{
    object? GetValue(string hiveAndPath, string name);
    IEnumerable<string> EnumerateSubKeys(string hiveAndPath);
    IEnumerable<string> EnumerateValueNames(string hiveAndPath);
}

public interface ICimReader
{
    IEnumerable<IDictionary<string, object?>> Query(string wql, string? scope = null);
}

public sealed record EventLogSummary(string LogName, int EntryCount, DateTimeOffset? LastWriteTimeUtc);
public interface IEventLogReader
{
    EventLogSummary? GetSummary(string logName);
}

public interface IFirewallReader
{
    IEnumerable<string> GetProfiles();
    IEnumerable<object> GetRules();
}

public interface IEnvReader
{
    string MachineName
    {
        get;
    }
    string OSVersionString
    {
        get;
    }
    bool Is64BitOS
    {
        get;
    }
    string UserName
    {
        get;
    }
    string UserDomainName
    {
        get;
    }
    IReadOnlyDictionary<string, string?> GetEnvironmentVariables();
}

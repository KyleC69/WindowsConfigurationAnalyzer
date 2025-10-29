namespace WindowsConfigurationAnalyzer.Contracts;

using Microsoft.Extensions.Logging;
using WindowsConfigurationAnalyzer.Infrastructure;
using WindowsConfigurationAnalyzer.Models;

public interface IAnalyzerModule
{
 string Name { get; }
 string Area { get; }
 Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken);
}

public interface IAnalyzerContext
{
 ILogger Logger { get; }
 ITimeProvider Time { get; }
 ActionLogger ActionLogger { get; }

 // Optional readers (stubs for now; implementations added later)
 IRegistryReader? Registry { get; }
 ICimReader? Cim { get; }
 IEventLogReader? EventLog { get; }
 IFirewallReader? Firewall { get; }
 IEnvReader? Environment { get; }
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
 string Id { get; }
 string Area { get; }
 Finding? Evaluate(AnalyzerResult result);
}

// Reader stubs (contracts only)
public interface IRegistryReader { }
public interface ICimReader { }
public interface IEventLogReader { }
public interface IFirewallReader { }
public interface IEnvReader { }

// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using Microsoft.Extensions.Logging;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;



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

	IRegistryReader Registry { get; }

	ICimReader Cim { get; }

	IEventLogReader EventLog { get; }

	IFirewallReader Firewall { get; }

	IEnvReader Environment { get; }
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
	string MachineName { get; }

	string OSVersionString { get; }

	bool Is64BitOS { get; }

	string UserName { get; }

	string UserDomainName { get; }

	IReadOnlyDictionary<string, string?> GetEnvironmentVariables();
}




// Manifest-based ETW Provider Abstraction (stubs to be implemented by runtime)
public interface IEventProvider
{
	// Emits a general action event mapped by taxonomy. Sequence helps keep IDs within area blocks.
	void EmitAction(string area, string action, string level, string message, string? exception, int sequence);





	// Session lifecycle helpers (map to1001/1002 per manifest)
	void EmitSessionStart(string sessionId, string computer, string version);
	void EmitSessionStop(string sessionId, int areas, int warnings, int errors, double elapsedSeconds);





	// Export completed helper (maps to11501)
	void EmitExportCompleted(string sessionId, string format, string path);
}
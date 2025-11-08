// Created:  2025/10/30
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Diagnostics.Eventing.Reader;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.EventLog;



public sealed class EventLogAnalyzer : IAnalyzerModule
{
	public string Name => "Event Log Analyzer";
	public string Area => "EventLog";





	public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
	{
		var area = Area;
		context.ActionLogger.Info(area, "Start", "Collecting event log inventory and summaries");
		var warnings = new List<string>();
		var errors = new List<string>();

		var logs = new List<object>();
		var scanned = 0;
		try
		{
			context.ActionLogger.Info(area, "EnumerateLogs", "Start");
			using var session = new EventLogSession();
			var names = session.GetLogNames();
			foreach (var name in names)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var entry = new Dictionary<string, object?>
				{
					["LogName"] = name
				};
				try
				{
					// Configuration
					using var cfg = new EventLogConfiguration(name, session);
					entry["IsEnabled"] = cfg.IsEnabled;
					entry["LogMode"] = cfg.LogMode.ToString();
					entry["MaximumSizeInBytes"] = cfg.MaximumSizeInBytes;
					entry["LogFilePath"] = cfg.LogFilePath;
					if (!cfg.IsEnabled)
					{
						// Only collect minimal info for disabled logs
						logs.Add(entry);

						continue;
					}
				}
				catch (Exception exCfg)
				{
					warnings.Add($"Config read failed for '{name}': {exCfg.Message}");
					entry["ConfigError"] = exCfg.Message;
				}

				// Info/record counts (prefer Eventing.Reader; fallback to classic for well-known logs)
				long? recordCount = null;
				try
				{
					var info = session.GetLogInformation(name, PathType.LogName);
					recordCount = info.RecordCount;
					entry["RecordCount"] = recordCount;
					entry["IsLogFull"] = info.IsLogFull;
					entry["FileSize"] = info.FileSize;
				}
				catch (Exception exInfo)
				{
					warnings.Add($"Info read failed for '{name}': {exInfo.Message}");
					entry["InfoError"] = exInfo.Message;
					// classic fallback for legacy logs
					try
					{
						using var ev = new System.Diagnostics.EventLog(name);
						entry["RecordCount"] = ev.Entries?.Count;
					}
					catch
					{
					}
				}

				// Recent severity counts (scan newest first, capped)
				try
				{
					var query = new EventLogQuery(name, PathType.LogName) { ReverseDirection = true };
					using var reader = new EventLogReader(query);
					var recent = new List<Dictionary<string, object?>>();
					var maxToScan = 1000; // cap to limit cost
					int countCritical = 0, countError = 0, countWarning = 0, countInfo = 0;
					DateTime? newest = null, oldest = null;
					for (var i = 0; i < maxToScan; i++)
					{
						using var rec = reader.ReadEvent();

						if (rec is null) break;

						if (i == 0) newest = rec.TimeCreated;

						oldest = rec.TimeCreated;
						var lvl = rec.Level; //1=Critical,2=Error,3=Warning,4=Info,5=Verbose
						if (lvl == 1)
							countCritical++;
						else if (lvl == 2)
							countError++;
						else if (lvl == 3)
							countWarning++;
						else if (lvl == 4) countInfo++;
						// Keep a very small sample of the newest few events metadata
						if (i < 20)
							recent.Add(new Dictionary<string, object?>
							{
								["Id"] = rec.Id,
								["Provider"] = rec.ProviderName,
								["Level"] = lvl,
								["TimeCreatedUtc"] = rec.TimeCreated?.ToUniversalTime(),
								["Keywords"] = rec.KeywordsDisplayNames?.ToArray(),
								["Task"] = rec.Task,
								["Opcode"] = rec.Opcode,
								["ActivityId"] = rec.ActivityId?.ToString()
							});
					}

					entry["RecentCritical"] = countCritical;
					entry["RecentErrors"] = countError;
					entry["RecentWarnings"] = countWarning;
					entry["RecentInformation"] = countInfo;
					entry["NewestEventUtc"] = newest?.ToUniversalTime();
					entry["OldestScannedUtc"] = oldest?.ToUniversalTime();
					entry["RecentSample"] = recent;
				}
				catch (Exception exScan)
				{
					warnings.Add($"Scan failed for '{name}': {exScan.Message}");
					errors.Add(exScan.ToString());
					context.ActionLogger.Error(area, "ScanLog", $"Scan failed for {name}", exScan);
					entry["ScanError"] = exScan.Message;
				}

				logs.Add(entry);
				scanned++;
				if (scanned % 25 == 0) context.ActionLogger.Info(area, "EnumerateLogs", $"Progress: {scanned} logs");
			}

			context.ActionLogger.Info(area, "EnumerateLogs", $"Complete: scanned={scanned}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Log enumeration failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "EnumerateLogs", "Log enumeration failed", ex);
		}

		// Add quick spotlight for classic core logs in case they were missing
		foreach (var core in new[] { "System", "Application", "Security" })
			if (!logs.Any(l => string.Equals((l as Dictionary<string, object?>)?["LogName"]?.ToString(), core,
				    StringComparison.OrdinalIgnoreCase)))
				try
				{
					using var ev = new System.Diagnostics.EventLog(core);
					logs.Add(new Dictionary<string, object?>
					{
						["LogName"] = core,
						["RecordCount"] = ev.Entries?.Count
					});
				}
				catch (Exception ex)
				{
					warnings.Add($"Core log fallback failed for '{core}': {ex.Message}");
				}

		var summary = new { Logs = logs.Count, Scanned = scanned };
		var details = new { Logs = logs };
		var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), warnings, errors);
		context.ActionLogger.Info(area, "Complete", "Event log inventory collected");

		return Task.FromResult(result);
	}
}
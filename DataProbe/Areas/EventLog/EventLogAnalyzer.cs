//  Created:  2025/10/30
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   EventLogAnalyzer.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Diagnostics.Eventing.Reader;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.EventLog;


public sealed class EventLogAnalyzer : IAnalyzerModule
{


    public IActivityLogger? _logger;


    public string Name => "Event Log Analyzer";
    public string Area => "EventLog";





    public Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger;
        string area = Area;
        _logger.Log(area, "Start", "Collecting event log inventory and summaries");
        List<string> warnings = [];
        List<string> errors = [];

        List<object> logs = [];
        int scanned = 0;
        try
        {
            _logger.Log(area, "EnumerateLogs", "Start");
            using EventLogSession session = new();
            IEnumerable<string>? names = session.GetLogNames();
            foreach (string name in names)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Dictionary<string, object?> entry = new()
                {
                    ["LogName"] = name
                };
                try
                {
                    // Configuration
                    using EventLogConfiguration cfg = new(name, session);
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
                    EventLogInformation? info = session.GetLogInformation(name, PathType.LogName);
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
                        using System.Diagnostics.EventLog ev = new(name);
                        entry["RecordCount"] = ev.Entries?.Count;
                    }
                    catch
                    {
                    }
                }

                // Recent severity counts (scan newest first, capped)
                try
                {
                    EventLogQuery query = new(name, PathType.LogName) { ReverseDirection = true };
                    using EventLogReader reader = new(query);
                    List<Dictionary<string, object?>> recent = [];
                    int maxToScan = 1000; // cap to limit cost
                    int countCritical = 0, countError = 0, countWarning = 0, countInfo = 0;
                    DateTime? newest = null, oldest = null;
                    for (int i = 0; i < maxToScan; i++)
                    {
                        using EventRecord? rec = reader.ReadEvent();

                        if (rec is null)
                        {
                            break;
                        }

                        if (i == 0)
                        {
                            newest = rec.TimeCreated;
                        }

                        oldest = rec.TimeCreated;
                        byte? lvl = rec.Level; //1=Critical,2=Error,3=Warning,4=Info,5=Verbose
                        if (lvl == 1)
                        {
                            countCritical++;
                        }
                        else if (lvl == 2)
                        {
                            countError++;
                        }
                        else if (lvl == 3)
                        {
                            countWarning++;
                        }
                        else if (lvl == 4)
                        {
                            countInfo++;
                        }

                        // Keep a very small sample of the newest few events metadata
                        if (i < 20)
                        {
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
                    _logger.Log("ERR", $"Scan failed for {name}", $"{area}");
                    entry["ScanError"] = exScan.Message;
                }

                logs.Add(entry);
                scanned++;
                if (scanned % 25 == 0)
                {
                    _logger.Log(area, "EnumerateLogs", $"Progress: {scanned} logs");
                }
            }

            _logger.Log(area, "EnumerateLogs", $"Complete: scanned={scanned}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Log enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Log enumeration failed: {ex.Message}", "Event Log Analyzer");
        }

        // Add quick spotlight for classic core logs in case they were missing
        foreach (string? core in new[] { "System", "Application", "Security" })
        {
            if (!logs.Any(l => string.Equals((l as Dictionary<string, object?>)?["LogName"]?.ToString(), core,
                    StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    using System.Diagnostics.EventLog ev = new(core);
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
            }
        }

        var summary = new { Logs = logs.Count, Scanned = scanned };
        var details = new { Logs = logs };
        AreaResult result = new(area, summary, details, new List<Finding>().AsReadOnly(), warnings, errors);
        _logger.Log(area, "Complete", "Event log inventory collected");

        return Task.FromResult(result);
    }


}
using System.Diagnostics;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Readers;

public sealed class EventLogReader : IEventLogReader
{
    public EventLogSummary? GetSummary(string logName)
    {
        try
        {
            using var ev = new EventLog(logName);
            if (!ev.Entries?.Cast<EventLogEntry>().Any() ?? true)
            {
                return new EventLogSummary(logName, 0, null);
            }

            var last = ev.Entries[ev.Entries.Count - 1];
            var lastUtc = DateTime.SpecifyKind(last.TimeGenerated, DateTimeKind.Local).ToUniversalTime();
            return new EventLogSummary(logName, ev.Entries.Count, lastUtc);
        }
        catch
        {
            return null;
        }
    }
}

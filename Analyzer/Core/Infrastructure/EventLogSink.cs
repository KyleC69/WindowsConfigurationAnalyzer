
using System.Diagnostics;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;

public sealed class EventLogSink
{
    private readonly string _source;
    private readonly string _logName;

    public EventLogSink(string source = "WindowsConfigurationAnalyzer", string logName = "Application")
    {
        _source = source;
        _logName = logName;
        try
        {
            if (!EventLog.SourceExists(_source))
            {
                // Creating a source requires admin; ignore failures silently per spec resilience
                EventLog.CreateEventSource(new EventSourceCreationData(_source, _logName));
            }
        }
        catch { /* never break analysis */ }
    }

    public void Write(string message, int eventId, EventLogEntryType type)
    {
        try
        {
            EventLog.WriteEntry(_source, message, type, eventId);
        }
        catch { /* never break analysis */ }
    }
}

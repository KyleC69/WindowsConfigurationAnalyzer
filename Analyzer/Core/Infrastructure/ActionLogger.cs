using System.Diagnostics;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Diagnostics.Etw;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using Microsoft.Extensions.Logging;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;

public interface ITimeProvider
{
    DateTimeOffset UtcNow
    {
        get;
    }
}

public interface IActionLogFile
{
    void Append(string line);
}

public sealed class FileActionLogSink : IActionLogFile
{
    private readonly string _path;
    public FileActionLogSink(string path) => _path = path;
    public void Append(string line)
    {
        try
        {
            // atomic-ish append; open/close per write ensures flush on crash
            System.IO.File.AppendAllText(_path, line + Environment.NewLine);
        }
        catch
        {
            // swallow: file logging must never break analysis
        }
    }
}

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class ActionLogger
{
    private readonly ILogger _logger;
    private readonly IActionLogFile? _fileSink;
    private readonly EventLogSink? _eventLogSink;
    private readonly List<ActionLogEntry> _entries = new();
    private readonly object _gate = new();
    private readonly Dictionary<string, int> _areaSeq = new(StringComparer.OrdinalIgnoreCase);

    public ActionLogger(ILogger logger, IActionLogFile? fileSink = null, EventLogSink? eventLogSink = null)
    {
        _logger = logger;
        _fileSink = fileSink;
        _eventLogSink = eventLogSink;
    }

    public IReadOnlyList<ActionLogEntry> Entries
    {
        get
        {
            lock (_gate)
            {
                return _entries.ToList();
            }
        }
    }

    public void Info(string area, string action, string message)
    => Write(area, action, "Information", message, null, (lvl, msg) => _logger.LogInformation("{Message}", msg));

    public void Warn(string area, string action, string message)
    => Write(area, action, "Warning", message, null, (lvl, msg) => _logger.LogWarning("{Message}", msg));

    public void Error(string area, string action, string message, Exception ex)
    => Write(area, action, "Error", message, ex.ToString(), (lvl, msg) => _logger.LogError(ex, "{Message}", msg));

    private void Write(string area, string action, string level, string message, string? exception, Action<string, string> sink)
    {
        var ts = DateTimeOffset.UtcNow;
        var entry = new ActionLogEntry(ts, area, action, level, message, exception);
        lock (_gate)
        {
            _entries.Add(entry);
        }

        // .NET structured logging
        sink(level, message);

        // File append
        _fileSink?.Append($"{ts:O}\t{area}\t{action}\t{level}\t{message}{(exception is null ? string.Empty : "\t" + exception)}");

        // ETW per taxonomy
        var areaCode = EtwAreaCodes.Resolve(area);
        var seq = NextSeq(area);
        if (string.Equals(level, "Information", StringComparison.OrdinalIgnoreCase))
        {
            WcaEventSource.Log.Analysis(areaCode, seq, area, action, message);
        }
        else if (string.Equals(level, "Warning", StringComparison.OrdinalIgnoreCase))
        {
            WcaEventSource.Log.Warning(area, action, message);
        }
        else if (string.Equals(level, "Error", StringComparison.OrdinalIgnoreCase))
        {
            WcaEventSource.Log.Error(area, action, message, exception ?? string.Empty);
        }

        // Windows Event Log redundancy
        try
        {
            if (_eventLogSink is not null)
            {
                var eventId = string.Equals(level, "Information", StringComparison.OrdinalIgnoreCase)
                ? ((areaCode * 1000) + (2 * 100) + seq)
                : string.Equals(level, "Warning", StringComparison.OrdinalIgnoreCase)
                ? 13301
                : 13401;
                var type = string.Equals(level, "Information", StringComparison.OrdinalIgnoreCase) ? EventLogEntryType.Information
                : string.Equals(level, "Warning", StringComparison.OrdinalIgnoreCase) ? EventLogEntryType.Warning
                : EventLogEntryType.Error;
                _eventLogSink.Write($"[{area}] {action}: {message}", eventId, type);
            }
        }
        catch { /* never break analysis */ }
    }

    private int NextSeq(string area)
    {
        lock (_gate)
        {
            if (!_areaSeq.TryGetValue(area, out var v))
            {
                v = -1;
            }

            v = (v + 1) % 100;
            _areaSeq[area] = v;
            return v;
        }
    }
}

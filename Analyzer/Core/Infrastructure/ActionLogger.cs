namespace WindowsConfigurationAnalyzer.Infrastructure;

using Microsoft.Extensions.Logging;
using WindowsConfigurationAnalyzer.Models;

public interface ITimeProvider
{
 DateTimeOffset UtcNow { get; }
}

public sealed class SystemTimeProvider : ITimeProvider
{
 public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class ActionLogger
{
 private readonly ILogger _logger;
 private readonly List<ActionLogEntry> _entries = new();
 private readonly object _gate = new();

 public ActionLogger(ILogger logger)
 {
 _logger = logger;
 }

 public IReadOnlyList<ActionLogEntry> Entries
 {
 get { lock (_gate) return _entries.ToList(); }
 }

 public void Info(string area, string action, string message)
 => Write(area, action, "Information", message, null, (lvl, msg) => _logger.LogInformation("{Message}", msg));

 public void Warn(string area, string action, string message)
 => Write(area, action, "Warning", message, null, (lvl, msg) => _logger.LogWarning("{Message}", msg));

 public void Error(string area, string action, string message, Exception ex)
 => Write(area, action, "Error", message, ex.ToString(), (lvl, msg) => _logger.LogError(ex, "{Message}", msg));

 private void Write(string area, string action, string level, string message, string? exception, Action<string,string> sink)
 {
 var entry = new ActionLogEntry(DateTimeOffset.UtcNow, area, action, level, message, exception);
 lock (_gate)
 {
 _entries.Add(entry);
 }
 sink(level, message);
 }
}

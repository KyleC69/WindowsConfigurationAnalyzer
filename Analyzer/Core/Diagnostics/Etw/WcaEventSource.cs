using System.Diagnostics.Tracing;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Diagnostics.Etw;

/// <summary>
/// EventSource provider for Windows Configuration Analyzer.
/// Provider Name: WCA.Diagnostics.Provider
/// Note: When using a manifest (.man) alongside EventSource, keep IDs and templates in sync with the manifest.
/// </summary>
[EventSource(Name = ProviderName, Guid = ProviderGuid)]
public sealed class WcaEventSource : EventSource
{
    public const string ProviderName = "WCA.Diagnostics.Provider";
    public const string ProviderGuid = "b8a6b9d0-0f21-4a3e-a5f6-6d9e7c5e7f34";

    public static WcaEventSource Log { get; } = new();

    [Flags]
    public enum Keywords : long
    {
        None = 0,
        Engine = 1 << 0,
        Security = 1 << 1,
        Performance = 1 << 2,
        Export = 1 << 3,
        Integration = 1 << 4,
        Readers = 1 << 5,
        Diagnostics = 1 << 6,
    }

    public enum Tasks
    {
        Control = 1,
        Discovery = 2,
        Analysis = 3,
        Warning = 4,
        Error = 5,
        Export = 6,
        Integration = 7,
    }

    [Event(1001, Level = EventLevel.Informational, Task = (EventTask)Tasks.Control, Opcode = EventOpcode.Start, Keywords = (EventKeywords)Keywords.Engine, Channel = EventChannel.Operational)]
    public void SessionStart(string sessionId, string computer, string version) => WriteEvent(1001, sessionId ?? string.Empty, computer ?? string.Empty, version ?? string.Empty);

    [Event(1002, Level = EventLevel.Informational, Task = (EventTask)Tasks.Control, Opcode = EventOpcode.Stop, Keywords = (EventKeywords)Keywords.Engine, Channel = EventChannel.Operational)]
    public void SessionStop(string sessionId, int areas, int warnings, int errors, double elapsedSeconds) => WriteEvent(1002, sessionId ?? string.Empty, areas, warnings, errors, elapsedSeconds);

    [Event(11501, Level = EventLevel.Informational, Task = (EventTask)Tasks.Export, Opcode = EventOpcode.Info, Keywords = (EventKeywords)(Keywords.Engine | Keywords.Export), Channel = EventChannel.Operational)]
    public void ExportCompleted(string sessionId, string format, string path) => WriteEvent(11501, sessionId ?? string.Empty, format ?? string.Empty, path ?? string.Empty);

    [Event(13301, Level = EventLevel.Warning, Task = (EventTask)Tasks.Warning, Opcode = EventOpcode.Info, Keywords = (EventKeywords)Keywords.Diagnostics, Channel = EventChannel.Operational)]
    public void Warning(string area, string action, string message) => WriteEvent(13301, area ?? string.Empty, action ?? string.Empty, message ?? string.Empty);

    [Event(13401, Level = EventLevel.Error, Task = (EventTask)Tasks.Error, Opcode = EventOpcode.Info, Keywords = (EventKeywords)Keywords.Diagnostics, Channel = EventChannel.Operational)]
    public void Error(string area, string action, string message, string exception) => WriteEvent(13401, area ?? string.Empty, action ?? string.Empty, message ?? string.Empty, exception ?? string.Empty);

    // Generic Control/Session info within any area block (TaskCode =0)
    [NonEvent]
    public void ControlInfo(int areaCode, int sequence, string area, string action, string message)
    {
        var eventId = (areaCode * 1000) + (0 * 100) + Math.Clamp(sequence, 0, 99);
        WriteEvent(eventId, area ?? string.Empty, action ?? string.Empty, message ?? string.Empty);
    }

    [NonEvent]
    public void Discovery(int areaCode, int sequence, string area, string action, string message)
    {
        var eventId = (areaCode * 1000) + (1 * 100) + Math.Clamp(sequence, 0, 99);
        WriteEvent(eventId, area ?? string.Empty, action ?? string.Empty, message ?? string.Empty);
    }

    [NonEvent]
    public void Analysis(int areaCode, int sequence, string area, string action, string message)
    {
        var eventId = (areaCode * 1000) + (2 * 100) + Math.Clamp(sequence, 0, 99);
        WriteEvent(eventId, area ?? string.Empty, action ?? string.Empty, message ?? string.Empty);
    }

    [NonEvent]
    public void ExportInfo(int areaCode, int sequence, string area, string format, string path)
    {
        var eventId = (areaCode * 1000) + (5 * 100) + Math.Clamp(sequence, 0, 99);
        WriteEvent(eventId, area ?? string.Empty, format ?? string.Empty, path ?? string.Empty);
    }

    [NonEvent]
    public void Integration(int areaCode, int sequence, string tool, string arguments, int exitCode)
    {
        var eventId = (areaCode * 1000) + (6 * 100) + Math.Clamp(sequence, 0, 99);
        WriteEvent(eventId, tool ?? string.Empty, arguments ?? string.Empty, exitCode);
    }
}

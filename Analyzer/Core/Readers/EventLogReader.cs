// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  EventLogReader.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Diagnostics;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Readers;



public sealed class EventLogReader : IEventLogReader
{
    public EventLogSummary? GetSummary(string logName)
    {
        try
        {
            using EventLog ev = new(logName);

            if (ev.Entries == null || ev.Entries.Count == 0)
            {
                return new EventLogSummary(logName, 0, null);
            }

            var lastIndex = ev.Entries.Count - 1;
            var last = ev.Entries[lastIndex];
            var lastUtc = DateTime.SpecifyKind(last.TimeGenerated, DateTimeKind.Local).ToUniversalTime();

            return new EventLogSummary(logName, ev.Entries.Count, lastUtc);
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (System.Security.SecurityException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (System.IO.IOException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }
}
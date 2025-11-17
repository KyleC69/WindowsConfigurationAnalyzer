// Created:  2025/11/11
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  EventLogRecordClone.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using System.Diagnostics.Eventing.Reader;
using System.Security.Principal;

using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Models;


public class EventLogRecordClone
{


    public EventLogRecordClone()
    {
    }





    // Clone constructor
    public EventLogRecordClone(EventLogRecord record)
    {
        if (record == null)
        {
            ActivityLogger.Log("Err", "EventLogRecord cannot be null.", "EventLogRecordClone");
        }

        Id = record!.Id;

        Level = record.Level;
        ProviderName = record.ProviderName ?? string.Empty;
        Message = record.FormatDescription() ?? string.Empty;
        TimeCreated = record.TimeCreated;
        Properties = record.Properties ?? new List<EventProperty>();
        ActivityId = record.ActivityId;
        RelatedActivityId = record.RelatedActivityId;
        LogName = record.LogName ?? string.Empty;
        MachineName = record.MachineName ?? string.Empty;
        OpcodeDisplayName = record?.OpcodeDisplayName ?? string.Empty;
        TaskDisplayName = record?.TaskDisplayName ?? string.Empty;
        LevelDisplayName = record?.LevelDisplayName ?? string.Empty;
        ProcessId = record?.ProcessId;
        ThreadId = record?.ThreadId;
        UserId = record?.UserId?.Value ?? string.Empty;

        // Additional EventLogRecord properties
        RecordId = record?.RecordId;
        Version = record?.Version;
        Qualifiers = record?.Qualifiers;
        Keywords = record?.Keywords;
        try
        {
            KeywordsDisplayNames = record?.KeywordsDisplayNames ?? Array.Empty<string>();
        }
        catch
        {
            KeywordsDisplayNames = Array.Empty<string>();
        }

        try
        {
            MatchedQueryIds = record?.MatchedQueryIds ?? Array.Empty<int>();
        }
        catch
        {
            MatchedQueryIds = Array.Empty<int>();
        }

    }





    public byte? Level { get; set; }


    public int Id { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime? TimeCreated { get; set; }

    public IList<EventProperty> Properties { get; set; } = new List<EventProperty>();

    public Guid? ActivityId { get; set; }

    public Guid? RelatedActivityId { get; set; }

    public string LogName { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    public string OpcodeDisplayName { get; set; } = string.Empty;

    public string TaskDisplayName { get; set; } = string.Empty;

    public string LevelDisplayName { get; set; } = string.Empty;

    public int? ProcessId { get; set; }

    public int? ThreadId { get; set; }

    public string UserId { get; set; } = string.Empty;

    // Added to mirror EventLogRecord
    public long? RecordId { get; set; }

    public byte? Version { get; set; }

    public int? Qualifiers { get; set; }

    public long? Keywords { get; set; }

    public IEnumerable<string> KeywordsDisplayNames { get; set; } = Array.Empty<string>();

    public IEnumerable<int> MatchedQueryIds { get; set; } = Array.Empty<int>();

    public EventBookmark? Bookmark { get; set; }

    public string ContainerLog { get; set; } = string.Empty;

    public Guid? ProviderId { get; set; }

    public int? Opcode { get; set; }

    public int? Task { get; set; }

    public SecurityIdentifier? UserIdSid { get; set; }





    // Implicit conversion from EventLogRecord to EventLogRecordClone
    public static implicit operator EventLogRecordClone(EventLogRecord record)
    {
        return new EventLogRecordClone(record);
    }


}
// Created:  2025/11/04
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  EventingViewModel.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Security.Principal;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.ViewModels;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

using Microsoft.UI.Xaml.Controls;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;



public partial class EventingViewModel : ObservableRecipient, INavigationAware
{
    private ListViewRecord? _corSelectedLogEvent;
    private int _hoursBack = 2;
    private bool _overrideLimit;
    // Search support
    private string? _searchText;
    // Selected event and its reflected properties
    private EventLogRecordClone? _selectedLogEvent;
    private string? _selectedLogName;





    public EventingViewModel()
    {
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
    }





    public ObservableCollection<string> LogNames
    {
        get;
    } = new();



    public string? SelectedLogName
    {
        get => _selectedLogName;
        set
        {
            if (SetProperty(ref _selectedLogName, value))
            {
                _ = LoadEventsForSelectedLogAsync();
            }
        }
    }



    public ObservableCollection<EventLogRecordClone> LogEvents { get; } = new();


    [DebuggerDisplay("{SelectedLogEvent}")]
    public EventLogRecordClone? SelectedLogEvent
    {
        get => _selectedLogEvent;
        set
        {
            //Search for correlated events based on selectected event's ActivityId
            if (SetProperty(ref _selectedLogEvent, value))
            {
                UpdateSelectedLogEventProperties();
            }
        }
    }



    public ObservableCollection<PropertyItem> SelectedLogEventProperties { get; } = new();



    public int HoursBack
    {
        get => _hoursBack;
        set => SetProperty(ref _hoursBack, value);
    }



    public string HoursBackText
    {
        get => HoursBack.ToString();
        set
        {
            if (int.TryParse(value, out var result))
            {
                HoursBack = result;
            }
        }
    }



    public bool OverrideLimit
    {
        get => _overrideLimit;
        set
        {
            if (SetProperty(ref _overrideLimit, value))
            {
                _ = LoadEventsForSelectedLogAsync();
            }
        }
    }



    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }



    public IAsyncRelayCommand RefreshCommand
    {
        get;
    }

    public IAsyncRelayCommand SearchCommand
    {
        get;
    }

    public ObservableCollection<EventLogRecord>? CorrelatedLogEvents
    {
        get;
        set;
    }



    public ListViewRecord? CorSelectedLogEvent
    {
        get => _corSelectedLogEvent;
        set => SetProperty(ref _corSelectedLogEvent, value);
    }





    public async void OnNavigatedTo(object parameter)
    {
        // Load the available logs when the page is navigated to
        await LoadEnabledLogNamesAsync();

        await LoadEventsForSelectedLogAsync();
    }





    public void OnNavigatedFrom()
    {
    }





    /// <summary>
    ///     Refreshes the available event logs and loads events for the currently selected log.
    /// </summary>
    /// <remarks>
    ///     This asynchronous method updates the list of enabled event logs and reloads the events
    ///     for the currently selected log. It is typically invoked to ensure the displayed data
    ///     is up-to-date.
    /// </remarks>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    private async Task RefreshAsync()
    {
        await LoadEnabledLogNamesAsync();
        await LoadEventsForSelectedLogAsync();
    }





    /// <summary>
    ///     Asynchronously loads the names of enabled event logs on the system.
    /// </summary>
    /// <remarks>
    ///     This method retrieves the names of event logs that are enabled and not of type
    ///     <see cref="System.Diagnostics.Eventing.Reader.EventLogType.Analytical" />.
    ///     If <see cref="OverrideLimit" /> is set to <c>true</c>, it ensures that the log has at least one event.
    ///     Otherwise, it checks if the log has at least one event within the time window specified by <see cref="HoursBack" />
    ///     .
    ///     The retrieved log names are added to the <see cref="LogNames" /> collection, and the first log name is set as the
    ///     <see cref="SelectedLogName" />.
    /// </remarks>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="System.Exception">
    ///     Thrown if an error occurs while retrieving the log names or accessing the event logs.
    /// </exception>
    public Task LoadEnabledLogNamesAsync()
    {
        List<string> names = new();
        EventLogSession session = new();
        List<string> activeLogs = new();
        var timeWindowMs = TimeSpan.FromHours(HoursBack).TotalMilliseconds;

        try
        {
            foreach (var logName in session.GetLogNames())
            {
                try
                {
                    EventLogConfiguration config = new(logName);

                    // Skip disabled logs and Analytical logs
                    if (!config.IsEnabled)
                    {
                        continue;
                    }

                    if (config.LogType == EventLogType.Analytical)
                    {
                        continue;
                    }



                    if (OverrideLimit)
                    {
                        //Ensures that the log has at least one event
                        EventLogQuery q = new(logName, PathType.LogName);

                        using EventLogReader reader2 = new(q);
                        if (reader2.ReadEvent() != null)
                        {
                            activeLogs.Add(logName);
                        }
                    }
                    else
                    {
                        // Check if the log has at least one event in the time window
                        var query = "*[System[TimeCreated[timediff(@SystemTime) <= " + timeWindowMs + "]]]";
                        EventLogQuery logQuery = new(logName, PathType.LogName, query);

                        using EventLogReader reader = new(logQuery);
                        if (reader.ReadEvent() != null)
                        {
                            activeLogs.Add(logName);
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }

                names.Add(logName);
            }

            activeLogs.ForEach(n => LogNames.Add(n));
            Debug.Assert(LogNames.Count > 0, "No active logs found.");
            SelectedLogName = LogNames.First();
        }
        catch (Exception e)
        {
            //Log to EventLog include the counts and variable names to the event payload.

            //Log to activity log
            Debug.WriteLine(e.Message);
        }

        return Task.CompletedTask;
    }





    /// <summary>
    ///     Asynchronously loads events from the specified active event log.
    /// </summary>
    /// <param name="logName">The name of the event log to load events from.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task LoadEventsFromActiveLogAsync(string logName)
    {


        try
        {
            var session = EventLogSession.GlobalSession;

            try
            {
                //EventLogRecord data;
                // Check if the log has at least one event by attempting to read one in reverse (newest first)
                EventLogQuery query = new(logName, PathType.LogName)
                {
                    ReverseDirection = true
                };
                Dictionary<string, string> eventProperties = new();

                using EventLogReader logReader = new(query);
                //while ((data = (EventLogRecord)logReader.ReadEvent()) != null) LogEvents.Add(data);

            }
            catch
            {
                // ignore inaccessible logs
            }
        }
        catch
        {
            // ignore session-level errors
        }

        return Task.CompletedTask;
    }





    private static string BuildTimediffQueryMilliseconds(long milliseconds)
    {
        // XPath for time window: timediff(@SystemTime) returns ms since event time
        // Pattern mirrors other usage in code: *[System[TimeCreated[timediff(@SystemTime) <= N]]]
        return $"*[System[TimeCreated[timediff(@SystemTime) <= {milliseconds}]]]";
    }





    private EventLogQuery BuildEventLogQuery(string logName, bool overrideAll)
    {
        if (overrideAll)
        // No time filter, but still newest-first via ReverseDirection later
        {
            return new EventLogQuery(logName, PathType.LogName);
        }

        // Convert hours to milliseconds safely using 64-bit arithmetic
        var milliseconds = HoursBack * 60L * 60L * 1000L;
        // If zero hours, produce a query that yields no results; fallback later will show all
        if (milliseconds <= 0)
        {
            milliseconds = 0;
        }

        var xPath = BuildTimediffQueryMilliseconds(milliseconds);

        return new EventLogQuery(logName, PathType.LogName, xPath);
    }





    /// <summary>
    ///     Asynchronously loads the events for the currently selected log.
    /// </summary>
    /// <remarks>
    ///     This method retrieves events from the log specified by <see cref="SelectedLogName" />.
    ///     If <see cref="OverrideLimit" /> is set to <c>true</c>, it retrieves all events from the log.
    ///     Otherwise, it retrieves events within the time window specified by <see cref="HoursBack" />.
    ///     The retrieved events are added to the <see cref="LogEvents" /> collection.
    /// </remarks>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="System.Exception">
    ///     Thrown if an error occurs while retrieving events from the selected log.
    /// </exception>
    private async Task LoadEventsForSelectedLogAsync()
    {
        LogEvents.Clear();

        if (string.IsNullOrWhiteSpace(SelectedLogName))
        {
            return;
        }

        var logName = SelectedLogName!;
        var overrideAll = OverrideLimit;

        // Collect results off the UI thread then batch add for fewer dispatcher hops.
        List<EventLogRecordClone> collected = new();

        await Task.Run(() =>
        {
            try
            {
                // Build query using helper to respect overrideAll and HoursBack snapshot.
                EventLogQuery queryObj = BuildEventLogQuery(logName, overrideAll);
                queryObj.ReverseDirection = true; // newest first

                using EventLogReader logReader = new(queryObj);
                EventRecord? rawRecord;

                while ((rawRecord = logReader.ReadEvent()) is EventLogRecord eventRecord)
                {
                    try
                    {
                        // Clone minimal data; heavy field access (e.g. FormatDescription) handled in clone with its own safeguards.
                        collected.Add(new EventLogRecordClone(eventRecord));
                    }
                    catch (Exception ex)
                    {
                        // Per-record issues should not terminate enumeration.
                        Debug.WriteLine($"Event clone failed (Log={logName}, RecordId={eventRecord.RecordId}): {ex.Message}");
                    }
                }
            }
            catch (EventLogException ele)
            {
                // Targeted exception type for event log issues.
                Debug.WriteLine($"Event log read failed (Log={logName}): {ele.Message}");
            }
            catch (Exception ex)
            {
                // Unexpected failure; still swallow to keep UI responsive but trace for diagnostics.
                Debug.WriteLine($"Unexpected error reading events (Log={logName}): {ex.Message}");
            }
        });

        if (collected.Count > 0)
        {
            // Use current thread dispatcher if available; avoid touching App.MainWindow in test context.
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (dispatcher is not null)
            {
                dispatcher.TryEnqueue(() =>
                {
                    foreach (var item in collected)
                    {
                        LogEvents.Add(item);
                    }
                });
            }
            else
            {
                foreach (var item in collected)
                {
                    LogEvents.Add(item);
                }
            }
        }
    }





    /// <summary>
    /// Handles the selection change event for the event list view.
    /// Updates the selected log event and loads correlated events if applicable.
    /// </summary>
    /// <param name="sender">The source of the event, typically the event list view.</param>
    /// <param name="args">The event data containing information about the selection change.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task OnEventListView_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {


        if (args.AddedItems?.Count > 0 && args.AddedItems[0] is ListViewRecord selectedItem)
        {

            // Find the actual EventLogRecord from the LogEvents collection using EventId and TimeCreated
            // as EventListItem might not contain the full EventLogRecord object.
            var fullEventRecord = await Task.Run(() =>
            {

                if (string.IsNullOrWhiteSpace(SelectedLogName))
                {
                    return null;
                }

                var timeWindowMs = (long)TimeSpan.FromHours(HoursBack).TotalMilliseconds;
                var query = BuildEventLogQuery(SelectedLogName, OverrideLimit);
                query.ReverseDirection = true; // newest first

                using EventLogReader logReader = new(query);

                EventLogRecord data;

                while ((data = (EventLogRecord)logReader.ReadEvent()) != null)
                {
                    if (data.Id == selectedItem.EventId && data.TimeCreated == selectedItem.TimeCreated)
                    {
                        return data;
                    }
                }

                return null;
            });

            SelectedLogEvent = fullEventRecord!;

            // Load correlated events based on the ActivityId of the selected event
            if (SelectedLogEvent?.ActivityId != Guid.Empty)
            {
                await LoadCorrelatedEventsAsync();
            }
            else
            {
                CorrelatedLogEvents?.Clear();
            }
        }
        else
        {
            SelectedLogEvent = null;
            CorrelatedLogEvents?.Clear();
        }
    }





    private Task LoadCorrelatedEventsAsync()
    {

        try
        {
            var parentEvent = SelectedLogEvent;
            if (parentEvent?.RelatedActivityId != Guid.Empty)
            {
                var related = EventLogSearcher.FindEventsWithGuid(parentEvent?.RelatedActivityId, LogNames);
                if (related.Count > 0)
                {
                    foreach (var r in related)
                    {
                        CorrelatedLogEvents?.Add(r);
                    }
                }
            }

            if (parentEvent?.ActivityId != Guid.Empty)
            {

                var activities = EventLogSearcher.FindEventsWithGuid(parentEvent?.ActivityId, LogNames);
                if (activities.Count > 0)
                {
                    foreach (var a in activities)
                    {
                        CorrelatedLogEvents?.Add(a);
                    }
                }
            }
        }
        catch (Exception)
        {

            //public to eventlog
            //Add activity logging
        }

        return Task.CompletedTask;
    }





    /// <summary>
    ///     Updates the properties of the currently selected log event.
    /// </summary>
    /// <remarks>
    ///     This method clears the existing properties and retrieves the public instance properties
    ///     of the selected log event using reflection. It then populates the <see cref="SelectedLogEventProperties" />
    ///     collection with the property names and their corresponding values.
    /// </remarks>
    /// <exception cref="Exception">
    ///     Reflection-related exceptions are caught and ignored to ensure the method does not fail
    ///     due to inaccessible or invalid property values.
    /// </exception>
    private void UpdateSelectedLogEventProperties()
    {
        SelectedLogEventProperties.Clear();

        if (SelectedLogEvent is null)
        {
            return;
        }

        try
        {
            foreach (var prop in SelectedLogEvent.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var name = prop.Name;
                string valueStr;
                try
                {
                    var val = prop.GetValue(SelectedLogEvent);
                    valueStr = val?.ToString() ?? string.Empty;
                }
                catch
                {
                    valueStr = string.Empty;
                }

                SelectedLogEventProperties.Add(new PropertyItem { Name = name, Value = valueStr });
            }
        }
        catch
        {

        }
    }





    /// <summary>
    ///     Searches through event logs for entries matching the specified search term.
    /// </summary>
    /// <remarks>
    ///     This method performs an asynchronous search operation across all available event logs.
    ///     It matches the search term against various fields of the event log entries, such as
    ///     provider name, event ID, level display name, description, and XML representation.
    /// </remarks>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    private async Task SearchAsync()
    {
        var term = SearchText;

        if (string.IsNullOrWhiteSpace(term))
        {
            return;
        }

        // Clear current results
        LogEvents.Clear();

        List<string> logsToSearch = LogNames.ToList();

        await Task.Run(() =>
        {
            foreach (var log in logsToSearch)
            {
                try
                {
                    EventLogQuery q = new(log, PathType.LogName)
                    {
                        ReverseDirection = true
                    };

                    using EventLogReader reader = new(q);
                    while (true)
                    {
                        using var record = reader.ReadEvent();

                        if (record is null)
                        {
                            break;
                        }

                        var match = false;
                        try
                        {
                            if (!match && record.ProviderName?.Contains(term, StringComparison.OrdinalIgnoreCase) ==
                                true)
                            {
                                match = true;
                            }

                            if (!match && record.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase))
                            {
                                match = true;
                            }

                            string? levelDisplay = null;
                            try
                            {
                                levelDisplay = record.LevelDisplayName;
                            }
                            catch
                            {
                            }

                            if (!match && !string.IsNullOrEmpty(levelDisplay) &&
                                levelDisplay.Contains(term, StringComparison.OrdinalIgnoreCase))
                            {
                                match = true;
                            }

                            string? desc = null;
                            try
                            {
                                desc = record.FormatDescription();
                            }
                            catch
                            {
                            }

                            if (!match && !string.IsNullOrEmpty(desc) &&
                                desc.Contains(term, StringComparison.OrdinalIgnoreCase))
                            {
                                match = true;
                            }

                            if (!match)
                            {
                                string? xml = null;
                                try
                                {
                                    xml = record.ToXml();
                                }
                                catch
                                {
                                }

                                if (!string.IsNullOrEmpty(xml) &&
                                    xml.Contains(term, StringComparison.OrdinalIgnoreCase))
                                {
                                    match = true;
                                }
                            }
                        }
                        catch
                        {
                            // ignore per-record matching issues
                        }

                        if (match)
                        {
                            string? levelDisplay = null;
                            try
                            {
                                levelDisplay = record.LevelDisplayName;
                            }
                            catch
                            {
                            }



                            //							App.MainWindow?.DispatcherQueue.TryEnqueue(() => LogEvents.Add(new EventLogRecordClone(item)));
                        }
                    }
                }
                catch
                {
                    // ignore bad/inaccessible logs
                }
            }
        });
    }





    public static class EventLogSearcher
    {
        public static List<EventLogRecord> FindEventsWithGuid(Guid? targetGuid, ICollection<string> logNames)
        {

            // Find all events in all logs that contain the target guid in the class properties or the EventProperties object.
            ConcurrentBag<EventLogRecord> matchingEvents = new();

            // If the target GUID is null or empty, we cannot search for it.
            if (!targetGuid.HasValue || targetGuid == Guid.Empty)
            {
                return [];
            }

            // Build an XPath query that attempts to pre-filter events that reference the GUID in common locations:
            // - ActivityID / RelatedActivityID
            // - EventData/Data elements
            // - UserData text
            // We still validate each candidate with EventContainsGuid to ensure accuracy.
            var guidString = targetGuid.Value.ToString();
            var guidUpper = guidString.ToUpperInvariant();
            var guidLower = guidString.ToLowerInvariant();

            // Compose a broad but still selective XPath; multiple OR branches combined at root with union via "or".
            // Note: Using contains(...) for string occurrences; exact matches first for efficiency.
            var xPath =
                $"*[(System[(ActivityID='{guidString}') or (RelatedActivityID='{guidString}')]) " +
                $"or (EventData[Data='{guidString}' or Data='{guidUpper}' or Data='{guidLower}' or Data[contains(.,'{guidString}')]]) " +
                $"or (UserData[.='{guidString}' or contains(.,'{guidString}')])]";

            // Parallelize across logs to reduce total wall-clock time.
            Parallel.ForEach(logNames,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                logName =>
                {
                    try
                    {
                        EventLogQuery query = new(logName, PathType.LogName, xPath)
                        {
                            ReverseDirection = true,
                            TolerateQueryErrors = true
                        };

                        using EventLogReader reader = new(query);
                        EventRecord? eventRecord;
                        while ((eventRecord = reader.ReadEvent()) != null)
                        {
                            if (eventRecord is EventLogRecord elr && EventContainsGuid(elr, targetGuid.Value))
                            {
                                matchingEvents.Add(elr);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //Public event and activity log
                    }
                });

            // Order results by newest first (ReverseDirection already gave us that per-log, but interleaving logs needs sorting).
            return matchingEvents
                .OrderByDescending(e => e.TimeCreated)
                .ToList();
        }





        private static bool EventContainsGuid(EventRecord record, Guid targetGuid)
        {
            try
            {
                if (record.ActivityId == targetGuid || record.RelatedActivityId == targetGuid)
                {
                    return true;
                }

                foreach (var prop in record.Properties)
                {
                    if (prop?.Value is Guid guid && guid == targetGuid)
                    {
                        return true;
                    }

                    if (prop?.Value is string str && Guid.TryParse(str, out var parsed) && parsed == targetGuid)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore malformed records
            }

            return false;
        }
    }





    public async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {

        try
        {
            // Handle the selection change event Expected type is EventLogRecordClone
            SelectedLogEvent = e.AddedItems[0] as EventLogRecordClone;
            await LoadEventsForSelectedLogAsync();
        }
        catch (Exception ex)
        {
            ActivityLogger.Log("Err", ex.Message, "Selector_OnSelectionChanged");
        }
    }
}



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

        Id = record.Id;

        Level = record.Level;
        ProviderName = record.ProviderName ?? string.Empty;
        Message = record.FormatDescription() ?? string.Empty;
        TimeCreated = record.TimeCreated;
        Properties = record.Properties ?? new List<EventProperty>();
        ActivityId = record.ActivityId;
        RelatedActivityId = record.RelatedActivityId;
        LogName = record.LogName ?? string.Empty;
        MachineName = record.MachineName ?? string.Empty;
        OpcodeDisplayName = record.OpcodeDisplayName ?? string.Empty;
        TaskDisplayName = record.TaskDisplayName ?? string.Empty;
        LevelDisplayName = record.LevelDisplayName ?? string.Empty;
        ProcessId = record.ProcessId;
        ThreadId = record.ThreadId;
        UserId = record.UserId?.Value ?? string.Empty;

        // Additional EventLogRecord properties
        RecordId = record.RecordId;
        Version = record.Version;
        Qualifiers = record.Qualifiers;
        Keywords = record.Keywords;
        try
        {
            KeywordsDisplayNames = record.KeywordsDisplayNames ?? Array.Empty<string>();
        }
        catch
        {
            KeywordsDisplayNames = Array.Empty<string>();
        }

        try
        {
            MatchedQueryIds = record.MatchedQueryIds ?? Array.Empty<int>();
        }
        catch
        {
            MatchedQueryIds = Array.Empty<int>();
        }

        Bookmark = record.Bookmark;
        ContainerLog = record.ContainerLog ?? string.Empty;
        ProviderId = record.ProviderId;
        Opcode = record.Opcode;
        Task = record.Task;
        UserIdSid = record.UserId;
    }





    public byte? Level
    {
        get;
        set;
    }




    public int Id
    {
        get;
        set;
    }

    public string ProviderName
    {
        get;
        set;
    } = string.Empty;

    public string Message
    {
        get;
        set;
    } = string.Empty;

    public DateTime? TimeCreated
    {
        get;
        set;
    }

    public IList<EventProperty> Properties
    {
        get;
        set;
    } = new List<EventProperty>();

    public Guid? ActivityId
    {
        get;
        set;
    }

    public Guid? RelatedActivityId
    {
        get;
        set;
    }

    public string LogName
    {
        get;
        set;
    } = string.Empty;

    public string MachineName
    {
        get;
        set;
    } = string.Empty;

    public string OpcodeDisplayName
    {
        get;
        set;
    } = string.Empty;

    public string TaskDisplayName
    {
        get;
        set;
    } = string.Empty;

    public string LevelDisplayName
    {
        get;
        set;
    } = string.Empty;

    public int? ProcessId
    {
        get;
        set;
    }

    public int? ThreadId
    {
        get;
        set;
    }

    public string UserId
    {
        get;
        set;
    } = string.Empty;

    // Added to mirror EventLogRecord
    public long? RecordId
    {
        get;
        set;
    }

    public byte? Version
    {
        get;
        set;
    }

    public int? Qualifiers
    {
        get;
        set;
    }

    public long? Keywords
    {
        get;
        set;
    }

    public IEnumerable<string> KeywordsDisplayNames
    {
        get;
        set;
    } = Array.Empty<string>();

    public IEnumerable<int> MatchedQueryIds
    {
        get;
        set;
    } = Array.Empty<int>();

    public EventBookmark? Bookmark
    {
        get;
        set;
    }

    public string ContainerLog
    {
        get;
        set;
    } = string.Empty;

    public Guid? ProviderId
    {
        get;
        set;
    }

    public int? Opcode
    {
        get;
        set;
    }

    public int? Task
    {
        get;
        set;
    }

    public SecurityIdentifier? UserIdSid
    {
        get;
        set;
    }





    // Implicit conversion from EventLogRecord to EventLogRecordClone
    public static implicit operator EventLogRecordClone(EventLogRecord record)
    {
        return new EventLogRecordClone(record);
    }
}




public class ListViewRecord
{
    public long? RecordId
    {
        get;
        set;
    }

    public int EventId
    {
        get;
        set;
    }

    public DateTime? TimeCreated
    {
        get;
        set;
    }

    public string? SourceDisplayName
    {
        get;
        set;
    }

    public byte? Level
    {
        get;
        set;
    }

    public string? LevelDisplayName
    {
        get;
        set;
    }

    // Description retained intentionally for potential future use, not shown in UI
    public string? Description
    {
        get;
        set;
    }

    public IList<EventProperty> Properties
    {
        get;
        set;
    } = new List<EventProperty>();
}




public class PropertyItem
{
    public string Name
    {
        get;
        set;
    } = string.Empty;

    public string Value
    {
        get;
        set;
    } = string.Empty;

    public string PropType
    {
        get;
        set;
    } = string.Empty;
}
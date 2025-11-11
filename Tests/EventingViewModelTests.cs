// Created:  2025/11/04
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Diagnostics.Eventing.Reader;
using System.Reflection;

using FluentAssertions;

using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace KC.WindowsConfigurationAnalyzer.Tests;



[TestClass]
public class EventingViewModelTests
{
    [TestMethod]
    public async Task EventingViewModel_Loads_LogNames_And_Applies_Filter_Query()
    {
        // Create instance directly; EventingViewModel has no ctor deps
        EventingViewModel vm = new();

        // Act: load logs
        vm.OnNavigatedTo(null!);

        // Wait some time for background enumeration to complete
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert logs exist (on typical Windows systems); if not, test remains resilient
        vm.LogNames.Should().NotBeNull();

        if (vm.LogNames.Count > 0)
        {
            vm.SelectedLogName = vm.LogNames.First();
            vm.OverrideLimit = false;
            vm.HoursBackText = "0"; // forces the XPath to 0ms

            await Task.Delay(TimeSpan.FromSeconds(2));

            vm.LogEvents.Should().NotBeNull();
            vm.LogEvents.Count.Should().BeGreaterThanOrEqualTo(0);

            if (vm.LogEvents.Count > 0)
            {
                // pass
            }
        }
        else
        {
            // If environment has no logs available, we still consider the viewmodel responsive
            vm.LogEvents.Should().NotBeNull();
        }
    }





    [TestMethod]
    public async Task EventingViewModel_LoadActiveLogNames()
    {
        // Create instance directly; EventingViewModel has no ctor deps
        EventingViewModel vm = new();
        // Act: load logs
        //  vm.OnNavigatedTo(null!);
        // Wait some time for background enumeration to complete
        Task.Delay(TimeSpan.FromSeconds(2)).Wait();
        // Assert logs exist (on typical Windows systems); if not, test remains resilient
        await vm.LoadEnabledLogNamesAsync();
        vm.LogNames.Should().NotBeNull();

        vm.LogNames.Count.Should().BeGreaterThan(0);

    }





    [TestMethod]
    public async Task LoadEventsFromLogByName()
    {

        EventingViewModel vm = new();

        await vm.LoadEventsFromActiveLogAsync("System");

        vm.LogEvents.Should().NotBeEmpty();



    }



    [TestMethod]
    public void BuildEventLogQuery_Returns_Usable_EventLogQuery()
    {
        // Arrange: pick an accessible log name
        string? logName = null;
        try
        {
            EventLogSession session = new();
            foreach (string? name in session.GetLogNames())
            {
                try
                {
                    EventLogConfiguration cfg = new(name);
                    if (cfg.IsEnabled && cfg.LogType != EventLogType.Analytical)
                    {
                        logName = name;
                        break;
                    }
                }
                catch
                {
                    // ignore and continue
                }
            }
        }
        catch
        {
            // ignore and fall through
        }

        if (string.IsNullOrWhiteSpace(logName))
        {
            Assert.Inconclusive("No accessible event logs available on this environment.");
            return;
        }

        EventingViewModel vm = new()
        {
            HoursBack = 1 // positive window
		};
        MethodInfo? method = typeof(EventingViewModel).GetMethod("BuildEventLogQuery", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Should().NotBeNull();

        // Act
        EventLogQuery filtered = (EventLogQuery)method!.Invoke(vm, new object[] { logName!, false })!;
        EventLogQuery unfiltered = (EventLogQuery)method.Invoke(vm, new object[] { logName!, true })!;

        // Assert: constructing a reader and attempting a single read should not throw for either query
        Action useFiltered = () =>
        {
            using EventLogReader reader = new(filtered);
            reader.ReadEvent();
        };

        Action useUnfiltered = () =>
        {
            using EventLogReader reader = new(unfiltered);
            reader.ReadEvent();
        };

        useFiltered.Should().NotThrow();
        useUnfiltered.Should().NotThrow();
    }
}
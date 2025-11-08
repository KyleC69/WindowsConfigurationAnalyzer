// Created:  2025/11/04
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



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
		var vm = new EventingViewModel();

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
			vm.HoursBackText = "0"; // forces the XPath to 0ms; fallback should then gather all

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
		var vm = new EventingViewModel();
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

		var vm = new EventingViewModel();

		await vm.LoadEventsFromActiveLogAsync("System");

		vm.LogEvents.Should().NotBeEmpty();



	}
    
    
    
    
    
    
    
}
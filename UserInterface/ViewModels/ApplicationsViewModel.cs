// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.ViewModels;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;



public partial class ApplicationsViewModel : ObservableRecipient, INavigationAware
{
	private readonly ISampleDataService _sampleDataService;

	private SampleOrder? _selected;





	public ApplicationsViewModel(ISampleDataService sampleDataService)
	{
		_sampleDataService = sampleDataService;
	}





	public SampleOrder? Selected
	{
		get => _selected;
		set => SetProperty(ref _selected, value);
	}



	public ObservableCollection<SampleOrder> SampleItems { get; } = [];





	public async void OnNavigatedTo(object parameter)
	{
		SampleItems.Clear();

		// TODO: Replace with real data.
		var data = await _sampleDataService.GetListDetailsDataAsync();

		foreach (var item in data) SampleItems.Add(item);
	}





	public void OnNavigatedFrom()
	{
	}





	public void EnsureItemSelected()
	{
		Selected ??= SampleItems.First();
	}
}
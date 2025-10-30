using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.ViewModels;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;

namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

public partial class DriversViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    [ObservableProperty]
    private SampleOrder? selected;

    public ObservableCollection<SampleOrder> SampleItems { get; private set; } = new ObservableCollection<SampleOrder>();

    public DriversViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        SampleItems.Clear();

        // TODO: Replace with real data.
        var data = await _sampleDataService.GetListDetailsDataAsync();

        foreach (var item in data)
        {
            SampleItems.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public void EnsureItemSelected()
    {
        Selected ??= SampleItems.First();
    }
}

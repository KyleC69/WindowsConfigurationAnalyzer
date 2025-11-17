// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  ShellViewModel.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using CommunityToolkit.Mvvm.ComponentModel;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;


public partial class ShellViewModel : ObservableRecipient
{


    private bool _isBackEnabled;

    private object? _selected;





    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }





    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }


    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }


    public INavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; }





    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        _isBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            _selected = NavigationViewService.SettingsItem;

            return;
        }

        NavigationViewItem? selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            _selected = selectedItem;
        }
    }


}
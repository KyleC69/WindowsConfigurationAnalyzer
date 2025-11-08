// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  INavigationService.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;



public interface INavigationService
{
    bool CanGoBack
    {
        get;
    }

    Frame? Frame
    {
        get;
        set;
    }

    event NavigatedEventHandler Navigated;

    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

    bool GoBack();
}
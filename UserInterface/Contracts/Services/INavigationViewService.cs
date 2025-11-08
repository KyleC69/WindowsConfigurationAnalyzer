// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using Microsoft.UI.Xaml.Controls;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;



public interface INavigationViewService
{
	IList<object>? MenuItems { get; }

	object? SettingsItem { get; }

	void Initialize(NavigationView navigationView);

	void UnregisterEvents();

	NavigationViewItem? GetSelectedItem(Type pageType);
}
// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using Microsoft.UI.Xaml;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;



public interface IThemeSelectorService
{
	ElementTheme Theme { get; }

	Task InitializeAsync();

	Task SetThemeAsync(ElementTheme theme);

	Task SetRequestedThemeAsync();
}
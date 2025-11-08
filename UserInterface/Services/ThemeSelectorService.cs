// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;
using Microsoft.UI.Xaml;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Services;



public class ThemeSelectorService : IThemeSelectorService
{
	private const string SettingsKey = "AppBackgroundRequestedTheme";

	private readonly ILocalSettingsService _localSettingsService;





	public ThemeSelectorService(ILocalSettingsService localSettingsService)
	{
		_localSettingsService = localSettingsService;
	}





	public ElementTheme Theme { get; set; } = ElementTheme.Default;





	public async Task InitializeAsync()
	{
		Theme = await LoadThemeFromSettingsAsync();
		await Task.CompletedTask;
	}





	public async Task SetThemeAsync(ElementTheme theme)
	{
		Theme = theme;

		await SetRequestedThemeAsync();
		await SaveThemeInSettingsAsync(Theme);
	}





	public async Task SetRequestedThemeAsync()
	{
		if (App.MainWindow.Content is FrameworkElement rootElement)
		{
			rootElement.RequestedTheme = Theme;

			TitleBarHelper.UpdateTitleBar(Theme);
		}

		await Task.CompletedTask;
	}





	private async Task<ElementTheme> LoadThemeFromSettingsAsync()
	{
		var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

		return Enum.TryParse(themeName, out ElementTheme cacheTheme) ? cacheTheme : ElementTheme.Default;
	}





	private async Task SaveThemeInSettingsAsync(ElementTheme theme)
	{
		await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());
	}
}
// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

using Microsoft.UI.Xaml;

using Windows.ApplicationModel;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;



public partial class SettingsViewModel : ObservableRecipient
{
    private readonly ILocalSettingsService _localSettings;
    // Plan:
    // - Replace [ObservableProperty] fields with explicit backing fields and public properties.
    // - Use SetProperty in setters to satisfy MVVMTK0045.
    // - Persist ExportPathTemplate and LogPathTemplate on change (moved logic from partial methods into setters).
    // - Keep remaining logic and patterns intact.

    private readonly IThemeSelectorService _themeSelectorService;

    private ElementTheme _elementTheme;

    private string? _exportPathTemplate;

    private string? _logPathTemplate;

    private string _versionDescription = string.Empty;





    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettings)
    {
        _themeSelectorService = themeSelectorService;
        _localSettings = localSettings;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(async param =>
        {
            if (_elementTheme != param)
            {
                _elementTheme = param;
                await _themeSelectorService.SetThemeAsync(param);
            }
        });

        _ = LoadAsync();
    }





    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }



    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }



    public string? ExportPathTemplate
    {
        get => _exportPathTemplate;
        set
        {
            if (SetProperty(ref _exportPathTemplate, value))
            {
                _ = _localSettings.SaveSettingAsync("ExportPathTemplate", value ?? string.Empty);
            }
        }
    }



    public string? LogPathTemplate
    {
        get => _logPathTemplate;
        set
        {
            if (SetProperty(ref _logPathTemplate, value))
            {
                _ = _localSettings.SaveSettingAsync("LogPathTemplate", value ?? string.Empty);
            }
        }
    }



    public ICommand SwitchThemeCommand
    {
        get;
    }





    private async Task LoadAsync()
    {
        _exportPathTemplate = await _localSettings.ReadSettingAsync<string>("ExportPathTemplate")
                              ?? "exports/" + Environment.MachineName + "/{yyyy-MM-dd}/{HHmm}.json";
        _logPathTemplate = await _localSettings.ReadSettingAsync<string>("LogPathTemplate")
                           ?? "logs/{yyyyMMdd-HHmm}.txt";
    }





    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMsix)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build,
                packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return
            $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
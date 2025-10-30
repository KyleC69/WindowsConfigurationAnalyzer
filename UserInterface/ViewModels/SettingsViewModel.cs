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
    // Plan:
    // - Replace [ObservableProperty] fields with explicit backing fields and public properties.
    // - Use SetProperty in setters to satisfy MVVMTK0045.
    // - Persist ExportPathTemplate and LogPathTemplate on change (moved logic from partial methods into setters).
    // - Keep remaining logic and patterns intact.

    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettings;

    private ElementTheme _elementTheme;
    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    private string _versionDescription = string.Empty;
    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    private string? _exportPathTemplate;
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

    private string? _logPathTemplate;
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

    public ICommand SwitchThemeCommand { get; }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettings)
    {
        _themeSelectorService = themeSelectorService;
        _localSettings = localSettings;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (_elementTheme != param)
                {
                    _elementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        _exportPathTemplate = await _localSettings.ReadSettingAsync<string>("ExportPathTemplate")
            ?? ("exports/" + Environment.MachineName + "/{yyyy-MM-dd}/{HHmm}.json");
        _logPathTemplate = await _localSettings.ReadSettingAsync<string>("LogPathTemplate")
            ?? "logs/{yyyyMMdd-HHmm}.txt";
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}

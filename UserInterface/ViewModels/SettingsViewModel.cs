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
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettings;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private string? _exportPathTemplate;

    [ObservableProperty]
    private string? _logPathTemplate;

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettings)
    {
        _themeSelectorService = themeSelectorService;
        _localSettings = localSettings;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        ExportPathTemplate = await _localSettings.ReadSettingAsync<string>("ExportPathTemplate")
            ?? ("exports/" + Environment.MachineName + "/{yyyy-MM-dd}/{HHmm}.json");
        LogPathTemplate = await _localSettings.ReadSettingAsync<string>("LogPathTemplate")
            ?? "logs/{yyyyMMdd-HHmm}.txt";
    }

    partial void OnExportPathTemplateChanged(string? value)
    {
        _ = _localSettings.SaveSettingAsync("ExportPathTemplate", value ?? string.Empty);
    }

    partial void OnLogPathTemplateChanged(string? value)
    {
        _ = _localSettings.SaveSettingAsync("LogPathTemplate", value ?? string.Empty);
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

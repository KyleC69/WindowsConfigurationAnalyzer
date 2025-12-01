//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   SettingsViewModel.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.Reflection;
using System.Runtime.Versioning;
using System.Windows.Input;

using Windows.ApplicationModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

using Microsoft.UI.Xaml;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;


[SupportedOSPlatform("windows10.0.22601.0")]
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
    private bool _isActivityLogEnabled;

    private string? _logPathTemplate;

    private string _versionDescription = string.Empty;





    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettings)
    {
        _themeSelectorService = themeSelectorService;
        _localSettings = localSettings;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(async void (param) =>
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
            if (SetProperty(ref _exportPathTemplate, value)) _ = _localSettings.SaveApplicationSettingAsync("ExportPathTemplate", value ?? string.Empty);
        }
    }


    public string? LogPathTemplate
    {
        get => _logPathTemplate;

        set
        {
            if (SetProperty(ref _logPathTemplate, value)) _ = _localSettings.SaveApplicationSettingAsync("LogPathTemplate", value ?? string.Empty);
        }
    }


    public ICommand SwitchThemeCommand { get; }


    /// <summary>
    ///     Gets or sets a value indicating whether activity logging is enabled in the application.
    /// </summary>
    /// <remarks>
    ///     When enabled, activity logging captures and stores application events for diagnostic or auditing purposes.
    ///     Changes to this property are persisted using the local settings service.
    /// </remarks>
    public bool IsActivityLoggingEnabled
    {
        get => _isActivityLogEnabled;

        set
        {
            if (SetProperty(ref _isActivityLogEnabled, value))
                // Persist as JSON-friendly lowercase boolean literal to align with deserialization logic
                _ = _localSettings.SaveApplicationSettingAsync("IsActivityLoggingEnabled", value ? "true" : "false");
        }
    }





    private async Task LoadAsync()
    {
        _exportPathTemplate = await _localSettings.ReadApplicationSettingAsync<string>("ExportPathTemplate")
                              ?? "exports/" + Environment.MachineName + "/{yyyy-MM-dd}/{HHmm}.json";
        _logPathTemplate = await _localSettings.ReadApplicationSettingAsync<string>("LogPathTemplate")
                           ?? "logs/{yyyyMMdd-HHmm}.txt";

        // Load Activity Logging setting (default to false if missing)
        var raw = await _localSettings.ReadApplicationSettingAsync<string>("IsActivityLoggingEnabled");
        var parsed = false;
        if (!string.IsNullOrWhiteSpace(raw))
        {
            // Accept JSON booleans (true/false) or string representations
            if (raw.Equals("true", StringComparison.OrdinalIgnoreCase))
                parsed = true;
            else if (raw.Equals("false", StringComparison.OrdinalIgnoreCase))
                parsed = false;
            else
                _ = bool.TryParse(raw, out parsed);
        }

        _isActivityLogEnabled = parsed;
        OnPropertyChanged(nameof(IsActivityLoggingEnabled));
    }





    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMsix)
        {
            PackageVersion packageVersion = Package.Current.Id.Version;

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
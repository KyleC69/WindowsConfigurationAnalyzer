// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;
using KC.WindowsConfigurationAnalyzer.UserInterface.Models;

using Microsoft.Extensions.Options;
using Microsoft.Windows.Storage;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Services;



public class LocalSettingsService : ILocalSettingsService
{
    private const string DefaultApplicationDataFolder = "Interface/ApplicationData";
    private const string DefaultLocalSettingsFile = "LocalSettings.json";
    private readonly string _applicationDataFolder;

    private readonly IFileService _fileService;

    private readonly string _localApplicationData =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private readonly string _localsettingsFile;
    private readonly LocalSettingsOptions _options;

    private bool _isInitialized;

    private IDictionary<string, object> _settings;





    public LocalSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
        _fileService = fileService;
        _options = options.Value;

        _applicationDataFolder = Path.Combine(_localApplicationData,
            _options.ApplicationDataFolder ?? DefaultApplicationDataFolder);
        _localsettingsFile = _options.LocalSettingsFile ?? DefaultLocalSettingsFile;

        _settings = new Dictionary<string, object>();
    }





    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        if (RuntimeHelper.IsMsix)
        {
            if (ApplicationData.GetDefault().LocalSettings.Values.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }
        else
        {
            await InitializeAsync();

            if (_settings != null && _settings.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }

        return default;
    }





    public async Task SaveSettingAsync<T>(string key, T value)
    {
        if (RuntimeHelper.IsMsix)
        {
            if (value is not null)
            {
                ApplicationData.GetDefault().LocalSettings.Values[key] = await Json.StringifyAsync(value);
            }
        }
        else
        {
            await InitializeAsync();

            if (value is not null)
            {
                _settings[key] = await Json.StringifyAsync(value);
            }

            await Task.Run(() => _fileService.Save(_applicationDataFolder, _localsettingsFile, _settings));
        }
    }





    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _settings = await Task.Run(() =>
                            _fileService.Read<IDictionary<string, object>>(_applicationDataFolder,
                                _localsettingsFile)) ??
                        new Dictionary<string, object>();

            _isInitialized = true;
        }
    }
}
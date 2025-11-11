// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  LocalSettingsService.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;

using Microsoft.Win32;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Services;

public class LocalSettingsService : ILocalSettingsService
{
    private readonly string _localFolderPath;
    private const string RegistryCompany = "KC";
    private const string RegistryProduct = "WindowsConfigurationAnalyzer";
    private bool _isInitialized;

    public LocalSettingsService()
    {
        _localFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), RegistryCompany, RegistryProduct);
        Directory.CreateDirectory(_localFolderPath);
    }

    public async Task SaveDataAsync<T>(string fileName, T data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        var fullPath = Path.Combine(_localFolderPath, fileName + ".json");
        var json = await KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers.Json.StringifyAsync(data);
        await File.WriteAllTextAsync(fullPath, json);
    }

    public async Task<T?> ReadDataAsync<T>(string filename)
    {
        var fullPath = Path.Combine(_localFolderPath, filename + ".json");
        if (!File.Exists(fullPath)) return default(T?);
        var json = await File.ReadAllTextAsync(fullPath);
        return await KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers.Json.ToObjectAsync<T>(json);
    }

    public async Task SaveObjectAsync<T>(string key, T obj)
    {
        await SaveDataAsync(key, obj);
    }

    public Task SaveApplicationSettingAsync(string key, string value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (value is null) throw new ArgumentNullException(nameof(value));
        using var k = OpenRegistryKey(true);
        k?.SetValue(key, value, RegistryValueKind.String);
        return Task.CompletedTask;
    }

    public async Task<T?> ReadApplicationSettingAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
        using var k = OpenRegistryKey(false);
        var raw = k?.GetValue(key) as string;
        if (string.IsNullOrEmpty(raw)) return default(T?);
        try
        {
            return await KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers.Json.ToObjectAsync<T>(raw);
        }
        catch
        {
            if (typeof(T) == typeof(string)) return (T?)(object?)raw;
            return default(T?);
        }
    }

    public async Task<StorageFile> SaveBinaryFileAsync(string fileName, byte[] data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Invalid file name", nameof(fileName));
        var fullPath = Path.Combine(_localFolderPath, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, data);
        return new StorageFile(fullPath);
    }

    public async Task<byte[]?> ReadBinaryFileAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
        var fullPath = Path.Combine(_localFolderPath, fileName);
        if (!File.Exists(fullPath)) return null;
        return await File.ReadAllBytesAsync(fullPath);
    }

    public async Task<byte[]?> ReadBytesFromFileAsync(StorageFile file)
    {
        if (file == null || string.IsNullOrEmpty(file.Path) || !File.Exists(file.Path)) return null;
        return await File.ReadAllBytesAsync(file.Path);
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        await Task.CompletedTask;
    }

    private async Task PersistModelAsync()
    {
        await Task.CompletedTask;
    }

    private RegistryKey? OpenRegistryKey(bool writable)
    {
        var subKey = $"Software\\{RegistryCompany}\\{RegistryProduct}";
        return writable ? Registry.CurrentUser.CreateSubKey(subKey, true) : Registry.CurrentUser.OpenSubKey(subKey, false);
    }
}
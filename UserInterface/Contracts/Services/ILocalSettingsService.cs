// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  ILocalSettingsService.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;



public interface ILocalSettingsService
{
    Task<T?> ReadSettingAsync<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);
}
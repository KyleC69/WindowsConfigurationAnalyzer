// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  IAppNotificationService.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Collections.Specialized;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;



public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();
}
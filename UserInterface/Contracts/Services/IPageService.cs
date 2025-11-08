// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  IPageService.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;



public interface IPageService
{
    Type GetPageType(string key);
}
// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  IActivationHandler.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder

namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;


public interface IActivationHandler
{


    bool CanHandle(object args);

    Task HandleAsync(object args);


}
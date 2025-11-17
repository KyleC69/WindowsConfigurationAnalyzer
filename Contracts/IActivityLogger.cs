// Created:  2025/11/12
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  Contracts.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IActivityLogger
{


    void Log(string level, string message, string context);

    void Info(string context, string action, string message);

    void Warning(string context, string action, string message);

    void Error(string context, string action, string message, Exception? ex);


}
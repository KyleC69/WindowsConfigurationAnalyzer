// Created:  2025/11/13
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  ActivityLogAdapter.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.Contracts;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


public class ActivityLogAdapter : IActivityLogger
{


    public void Log(string level, string message, string context)
    {
        ActivityLogger.Log(level, message, context);
    }





    public void Info(string context, string action, string message)
    {
        ActivityLogger.Log("INF", message, context + action);
    }





    public void Warning(string context, string action, string message)
    {
        ActivityLogger.Log("WRN", message, context + action);
    }





    public void Error(string context, string action, string message, Exception? ex)
    {
        ActivityLogger.Log("ERR", message, context + action);
    }


}
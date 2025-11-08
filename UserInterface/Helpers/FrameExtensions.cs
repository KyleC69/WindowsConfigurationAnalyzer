// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  FrameExtensions.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using Microsoft.UI.Xaml.Controls;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;



public static class FrameExtensions
{
    public static object? GetPageViewModel(this Frame frame)
    {
        return frame?.Content?.GetType().GetProperty("ViewModel")?.GetValue(frame.Content, null);
    }
}
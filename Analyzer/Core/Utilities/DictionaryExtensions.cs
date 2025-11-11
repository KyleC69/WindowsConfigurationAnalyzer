// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  DictionaryExtensions.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;



public static class DictionaryExtensions
{
    public static object? GetOrDefault(this IDictionary<string, object?> dict, string key)
    {
        return dict.TryGetValue(key, out object? v) ? v : null;
    }





    public static T? GetAs<T>(this IDictionary<string, object?> dict, string key)
    {
        return dict.TryGetValue(key, out object? v) && v is T t ? t : default(T?);
    }
}
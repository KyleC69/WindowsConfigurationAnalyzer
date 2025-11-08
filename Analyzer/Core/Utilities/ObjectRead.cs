// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  ObjectRead.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Reflection;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;



public static class ObjectRead
{
    public static bool TryGetProperty(object? obj, string name, out object? value)
    {
        value = null;

        if (obj is null)
        {
            return false;
        }

        if (obj is IDictionary<string, object?> dict)
        {
            return dict.TryGetValue(name, out value);
        }

        var type = obj.GetType();
        var prop =
            type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (prop is null)
        {
            return false;
        }

        value = prop.GetValue(obj);

        return true;
    }





    public static T? GetPropertyAs<T>(object? obj, string name)
    {
        if (TryGetProperty(obj, name, out var v) && v is T t)
        {
            return t;
        }

        try
        {
            // Handle numeric conversions where possible
            if (TryGetProperty(obj, name, out var v2) && v2 is not null)
            {
                return (T)Convert.ChangeType(v2, typeof(T));
            }
        }
        catch
        {
        }

        return default(T?);
    }
}
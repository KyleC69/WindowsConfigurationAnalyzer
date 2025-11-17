// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface.Core
// File:  Json.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using System.Globalization;

using Newtonsoft.Json;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers;


public static class Json
{


    public static async Task<T?> ToObjectAsync<T>(string value)
    {
        if (value is null)
        {
            return default;
        }

        // Fast-path for string targets: value is already the desired result
        if (typeof(T) == typeof(string))
        {
            return (T)(object)value;
        }

        var trimmed = value.Trim();

        // If target is an enum, try parsing directly from the token (common case like "Dark")
        if (typeof(T).IsEnum)
        {
            if (Enum.TryParse(typeof(T), trimmed, true, out var enumVal))
            {
                return (T)enumVal;
            }
        }

        // Detect whether the input already looks like JSON. If not, quote it so Json.NET can parse it as a string.
        var looksLikeJson = trimmed.StartsWith("{") || trimmed.StartsWith("[") || trimmed.StartsWith("\"")
                            || string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(trimmed, "null", StringComparison.OrdinalIgnoreCase)
                            || double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out _);

        var jsonInput = value;
        if (!looksLikeJson)
        {
            jsonInput = '"' + trimmed.Replace("\"", "\\\"") + '"';
        }

        try
        {
            // Keep asynchronous signature; offload deserialization to thread pool for heavier types
            return await Task.Run(() => JsonConvert.DeserializeObject<T>(jsonInput));
        }
        catch (JsonReaderException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize value as {typeof(T).Name}: {value}", ex);
        }
    }





    public static async Task<string> StringifyAsync(object value)
    {
        return await Task.Run<string>(() => JsonConvert.SerializeObject(value));
    }


}
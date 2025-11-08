// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface.Core
// File:  Json.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using Newtonsoft.Json;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers;



public static class Json
{
    public static async Task<T?> ToObjectAsync<T>(string value)
    {
        return await Task.Run<T>(() => JsonConvert.DeserializeObject<T>(value) ?? throw new InvalidOperationException());
    }





    public static async Task<string> StringifyAsync(object value)
    {
        return await Task.Run<string>(() => JsonConvert.SerializeObject(value));
    }
}
// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  EnvironmentReader.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using System.Collections;

using KC.WindowsConfigurationAnalyzer.Contracts;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public sealed class EnvironmentReader : IEnvReader
{


    public string MachineName => Environment.MachineName;
    public string OSVersionString => Environment.OSVersion.VersionString;
    public bool Is64BitOS => Environment.Is64BitOperatingSystem;
    public string UserName => Environment.UserName;
    public string UserDomainName => Environment.UserDomainName;





    public IReadOnlyDictionary<string, string?> GetEnvironmentVariables()
    {
        Dictionary<string, string?> dict = new();
        foreach (DictionaryEntry kvp in Environment.GetEnvironmentVariables())
        {
            var key = kvp.Key?.ToString() ?? string.Empty;
            var val = kvp.Value?.ToString();
            if (!dict.ContainsKey(key))
            {
                dict[key] = val;
            }
        }

        return dict;
    }


}
// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  RegistryReader.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.Contracts;

using Microsoft.Win32;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public sealed class RegistryReader : IRegistryReader
{


    public object? GetValue(string hiveAndPath, string name)
    {
        using RegistryKey? key = OpenSubKey(hiveAndPath);

        return key?.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
    }





    public IEnumerable<string> EnumerateSubKeys(string hiveAndPath)
    {
        using RegistryKey? key = OpenSubKey(hiveAndPath);

        return key?.GetSubKeyNames() ?? [];
    }





    public IEnumerable<string> EnumerateValueNames(string hiveAndPath)
    {
        using RegistryKey? key = OpenSubKey(hiveAndPath);

        return key?.GetValueNames() ?? [];
    }





    private static RegistryKey? OpenSubKey(string hiveAndPath)
    {
        var (hive, path) = SplitHive(hiveAndPath);
        RegistryKey? baseKey = hive switch
        {
            "HKLM" or "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
            "HKCU" or "HKEY_CURRENT_USER" => Registry.CurrentUser,
            "HKCR" or "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
            "HKU" or "HKEY_USERS" => Registry.Users,
            "HKCC" or "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
            _ => null
        };

        return baseKey?.OpenSubKey(path, false);
    }





    private static (string hive, string path) SplitHive(string hiveAndPath)
    {
        var idx = hiveAndPath.IndexOf('\\');

        if (idx < 0)
        {
            return (hiveAndPath, string.Empty);
        }

        return (hiveAndPath.Substring(0, idx), hiveAndPath[(idx + 1)..]);
    }


}
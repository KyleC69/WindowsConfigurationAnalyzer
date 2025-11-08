// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  CimReader.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Management;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Readers;



public sealed class CimReader : ICimReader
{
    public IEnumerable<IDictionary<string, object?>> Query(string wql, string? scope = null)
    {
        var s = string.IsNullOrWhiteSpace(scope)
            ? new ManagementScope("\\\\.\\root\\cimv2")
            : new ManagementScope(scope);
        s.Connect();
        using ManagementObjectSearcher searcher = new(s, new ObjectQuery(wql));
        foreach (var o in searcher.Get())
        {
            ManagementObject? mo = (ManagementObject)o;
            Dictionary<string, object?> dict = new(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in mo.Properties)
            {
                dict[prop.Name] = prop.Value;
            }

            yield return dict;
        }
    }
}
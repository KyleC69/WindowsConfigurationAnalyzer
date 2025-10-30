using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Readers;

public sealed class EnvironmentReader : IEnvReader
{
    public string MachineName => Environment.MachineName;
    public string OSVersionString => Environment.OSVersion.VersionString;
    public bool Is64BitOS => Environment.Is64BitOperatingSystem;
    public string UserName => Environment.UserName;
    public string UserDomainName => Environment.UserDomainName;

    public IReadOnlyDictionary<string, string?> GetEnvironmentVariables()
    {
        var dict = new Dictionary<string, string?>();
        foreach (System.Collections.DictionaryEntry kvp in Environment.GetEnvironmentVariables())
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

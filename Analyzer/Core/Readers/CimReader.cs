using System.Management;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Readers;

public sealed class CimReader : ICimReader
{
    public IEnumerable<IDictionary<string, object?>> Query(string wql, string? scope = null)
    {
        var s = string.IsNullOrWhiteSpace(scope) ? new ManagementScope("\\\\.\\root\\cimv2") : new ManagementScope(scope);
        s.Connect();
        using var searcher = new ManagementObjectSearcher(s, new ObjectQuery(wql));
        foreach (ManagementObject mo in searcher.Get())
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in mo.Properties)
            {
                dict[prop.Name] = prop.Value;
            }
            yield return dict;
        }
    }
}

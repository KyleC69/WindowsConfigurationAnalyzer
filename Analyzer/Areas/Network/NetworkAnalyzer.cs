using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Network;

public sealed class NetworkAnalyzer : IAnalyzerModule
{
    public string Name => "Network Analyzer";
    public string Area => "Network";

    public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
    {
        var area = Area;
        context.ActionLogger.Info(area, "Collect", "Collecting network configuration via readers");

        var adapters = new List<object>();
        try
        {
            foreach (var n in context.Cim.Query("SELECT Description, MACAddress, IPEnabled, DHCPEnabled, IPAddress FROM Win32_NetworkAdapterConfiguration"))
            {
                var ips = n.GetAs<string[]>("IPAddress") ?? Array.Empty<string>();
                adapters.Add(new
                {
                    Description = n.GetOrDefault("Description"),
                    MAC = n.GetOrDefault("MACAddress"),
                    IPEnabled = n.GetOrDefault("IPEnabled"),
                    DHCPEnabled = n.GetOrDefault("DHCPEnabled"),
                    IPAddresses = ips
                });
            }
        }
        catch (Exception ex)
        {
            context.ActionLogger.Warn(area, "Collect", $"CIM query failed: {ex.Message}");
        }

        var profiles = context.Firewall.GetProfiles();
        var rules = context.Firewall.GetRules();
        var summary = new { Adapters = adapters.Count, FirewallProfiles = profiles };
        var details = new { Adapters = adapters, FirewallProfiles = profiles, FirewallRules = rules };
        var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), Array.Empty<string>(), Array.Empty<string>());
        context.ActionLogger.Info(area, "Collect", "Network configuration collected");
        return Task.FromResult(result);
    }
}

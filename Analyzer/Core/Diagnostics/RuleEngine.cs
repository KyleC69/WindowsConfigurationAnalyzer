// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  RuleEngine.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Diagnostics;



public sealed class RuleEngine
{
    private readonly IReadOnlyList<IRule> _rules;





    public RuleEngine(IEnumerable<IRule> rules)
    {
        _rules = rules.ToList();
    }





    public IReadOnlyList<Finding> Evaluate(AnalyzerResult result)
    {
        List<Finding> findings = new();
        foreach (var rule in _rules)
        {
            var f = rule.Evaluate(result);
            if (f is not null)
            {
                findings.Add(f);
            }
        }

        return findings;
    }
}




// Starter rules
public sealed class AvMissingRule : IRule
{
    public string Id => "SEC-AV-MISSING";
    public string Area => "Security";





    public Finding? Evaluate(AnalyzerResult result)
    {
        var sec = result.Areas.FirstOrDefault(a => a.Area == "Security");

        return sec?.Details is IDictionary<string, object?> d && (!d.ContainsKey("Antivirus") || d["Antivirus"] is null)
            ? new Finding("Critical", "Antivirus product not detected")
            : null;
    }
}




public sealed class DuplicateDnsRule : IRule
{
    public string Id => "NET-DNS-DUP";
    public string Area => "Network";





    public Finding? Evaluate(AnalyzerResult result)
    {
        var net = result.Areas.FirstOrDefault(a => a.Area == "Network");
        if (net?.Details is IDictionary<string, object?> d && d.TryGetValue("Adapters", out var adaptersObj) &&
            adaptersObj is IEnumerable<object> adapters)
        {
            List<string> allDns = new();
            foreach (var a in adapters)
            {
                if (a is IDictionary<string, object?> ad)
                {
                    if (ad.TryGetValue("IPAddresses", out var ips) && ips is IEnumerable<string> list)
                    {
                        allDns.AddRange(list);
                    }
                }
            }

            if (allDns.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                return new Finding("Warning", "Duplicate IP or DNS entries detected");
            }
        }

        return null;
    }
}




// New comprehensive rule set (initial pass)
public sealed class FirewallDisabledRule : IRule
{
    public string Id => "SEC-FW-DISABLED";
    public string Area => "Security";





    public Finding? Evaluate(AnalyzerResult result)
    {
        var sec = result.Areas.FirstOrDefault(a => a.Area == "Security");
        if (sec?.Details is IDictionary<string, object?> d)
        {
            if (d.TryGetValue("UAC", out _))
            {
                /* ignore */
            }

            IDictionary<string, object?>? fwProfiles =
                result.Areas.FirstOrDefault(a => a.Area == "Network")?.Details as IDictionary<string, object?>;

            if (fwProfiles is not null && fwProfiles.TryGetValue("FirewallProfiles", out var prof) &&
                prof is IEnumerable<string> p)
            {
                if (!p.Any())
                {
                    return new Finding("Warning", "No active firewall profiles detected");
                }
            }
        }

        return null;
    }
}




public sealed class HighCpuRule : IRule
{
    public string Id => "PERF-CPU-HIGH";
    public string Area => "Performance";





    public Finding? Evaluate(AnalyzerResult result)
    {
        var perf = result.Areas.FirstOrDefault(a => a.Area == "Performance");

        if (perf?.Details is IDictionary<string, object?> d && d.TryGetValue("CpuPercent", out var v))
        {
            if (double.TryParse(v?.ToString(), out var cpu) && cpu >= 90)
            {
                return new Finding("Warning", $"High CPU usage detected: {cpu}%");
            }
        }

        return null;
    }
}




public sealed class LowMemoryRule : IRule
{
    public string Id => "PERF-MEM-LOW";
    public string Area => "Performance";





    public Finding? Evaluate(AnalyzerResult result)
    {
        var perf = result.Areas.FirstOrDefault(a => a.Area == "Performance");

        if (perf?.Details is IDictionary<string, object?> d && d.TryGetValue("MemoryUsedPercent", out var v))
        {
            if (double.TryParse(v?.ToString(), out var used) && used >= 90)
            {
                return new Finding("Warning", $"High memory utilization detected: {used}%");
            }
        }

        return null;
    }
}




public sealed class SuspiciousAutorunsRule : IRule
{
    public string Id => "STARTUP-SUSPICIOUS";
    public string Area => "Startup";





    public Finding? Evaluate(AnalyzerResult result)
    {
        var s = result.Areas.FirstOrDefault(a => a.Area == "Startup");
        if (s?.Details is IDictionary<string, object?> d)
        {
            if (d.TryGetValue("IFEO", out var ifeo) && ifeo is IEnumerable<object> list && list.Any())
            {
                return new Finding("Warning", "Image File Execution Options debuggers present");
            }

            if (d.TryGetValue("WmiSubscriptions", out var wmi) && wmi is IEnumerable<object> wlist && wlist.Any())
            {
                return new Finding("Warning", "WMI Event Subscriptions found (check for persistence)");
            }
        }

        return null;
    }
}




public sealed class RdpExposedRule : IRule
{
    public string Id => "SEC-RDP-EXPOSED";
    public string Area => "Security";





    public Finding? Evaluate(AnalyzerResult result)
    {
        var pol = result.Areas.FirstOrDefault(a => a.Area == "Policy/GPO");
        if (pol?.Details is IDictionary<string, object?> d && d.TryGetValue("Policies", out var pols) &&
            pols is IDictionary<string, object?> pd)
        {
            var deny = GetInt(pd,
                "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\Terminal Services:fDenyTSConnections");

            if (deny is int i && i == 0)
            {
                return new Finding("Warning", "RDP connections allowed by policy");
            }
        }

        return null;
    }





    private static object? GetInt(IDictionary<string, object?> d, string key)
    {
        return d.TryGetValue(key, out var v) ? v : null;
    }
}
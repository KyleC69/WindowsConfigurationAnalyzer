using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Diagnostics;

public sealed class RuleEngine
{
    private readonly IReadOnlyList<IRule> _rules;
    public RuleEngine(IEnumerable<IRule> rules) => _rules = rules.ToList();
    public IReadOnlyList<Finding> Evaluate(AnalyzerResult result)
    {
        var findings = new List<Finding>();
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
        if (sec?.Details is IDictionary<string, object?> d && (!d.ContainsKey("Antivirus") || d["Antivirus"] is null))
        {
            return new Finding("Critical", "Antivirus product not detected");
        }

        return null;
    }
}

public sealed class DuplicateDnsRule : IRule
{
    public string Id => "NET-DNS-DUP";
    public string Area => "Network";
    public Finding? Evaluate(AnalyzerResult result)
    {
        var net = result.Areas.FirstOrDefault(a => a.Area == "Network");
        if (net?.Details is IDictionary<string, object?> d && d.TryGetValue("Adapters", out var adaptersObj) && adaptersObj is IEnumerable<object> adapters)
        {
            var allDns = new List<string>();
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

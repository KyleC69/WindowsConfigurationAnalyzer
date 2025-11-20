using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;


public class RuleResult
{
    public string RuleName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int SeverityScore { get; set; }
    public string SchemaVersion { get; set; } = string.Empty;
}
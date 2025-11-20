using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;

public class WorkflowParameters
{
    // Shared context values
    public string ProviderName { get; set; } = string.Empty;
    public string ManifestPath { get; set; } = string.Empty;
    public string[] RegistryKeys { get; set; } = Array.Empty<string>();
    public string[] RegisteredProviders { get; set; } = Array.Empty<string>();
    public string[] ChannelNamesFromManifest { get; set; } = Array.Empty<string>();
    public string[] ChannelNamesFromRegistry { get; set; } = Array.Empty<string>();
    public string FileAcl { get; set; } = string.Empty;
    public string RegistryAcls { get; set; } = string.Empty;
    public string WevtutilQuery { get; set; } = string.Empty;
    public DateTime ProbeTimestamp { get; set; }
}

public class WorkflowConstraints
{
    public bool RunSequentially { get; set; } = true;
    public bool StopOnFailure { get; set; } = false;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}

public class WorkflowResult
{
    public string RuleName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int SeverityScore { get; set; }
    public string SchemaVersion { get; set; } = string.Empty;
}
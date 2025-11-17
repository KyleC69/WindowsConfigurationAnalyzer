namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Models;

// Canonical ACE representation for files and registry keys
public record AclEntry
{
    public string IdentityReference { get; init; } = string.Empty;
    public string Rights { get; init; } = string.Empty; // "Read;Write;FullControl"
    public string InheritanceFlags { get; init; } = string.Empty;
    public bool IsInherited { get; init; }
    public string RawSddl { get; init; } = string.Empty;
}

// Registry key/value snapshot
public record RegistryValueEntry
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public record RegistryKeySnapshot
{
    public string KeyPath { get; init; } = string.Empty;
    public List<RegistryValueEntry> Values { get; init; } = new();
    public List<AclEntry> Acls { get; init; } = new();
    public string RawAclSddl { get; init; } = string.Empty;
}

// Parsed/normalized manifest structure (light)
public record ManifestSnapshot
{
    public string Path { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public Dictionary<string, string> Properties { get; init; } = new(); // e.g., provider name
    public List<string> ChannelNames { get; init; } = new();
    public string RawXml { get; init; } = string.Empty;
}

// Top-level facts object passed to rules engine
public record ProbeFacts
{
    public string ProviderName { get; init; } = string.Empty;
    public string ManifestPath { get; init; } = string.Empty;
    public ManifestSnapshot? Manifest { get; init; }
    public List<RegistryKeySnapshot> RegistrySnapshots { get; init; } = new();
    public List<string> RegisteredProviders { get; init; } = new();
    public List<AclEntry> FileAcl { get; init; } = new();
    public List<AclEntry> RegistryAcls { get; init; } = new();
    public string WevtutilOutput { get; init; } = string.Empty;
    public string ProbeTimestamp { get; init; } = DateTime.UtcNow.ToString("o");
    public Dictionary<string, object> Extra { get; init; } = new();
}

// Audit-grade rule result artifact (wrapper for RulesEngine output + evidence)
public record RuleResultArtifact
{
    public string WorkflowName { get; init; } = string.Empty;
    public string RunId { get; init; } = Guid.NewGuid().ToString();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public ProbeFacts Facts { get; init; } = new();
    public object RawRuleResultTree { get; init; } = new(); // store engine output as-is
    public Dictionary<string, object> HelperOutputs { get; init; } = new();
    public List<string> EvidenceFiles { get; init; } = new();
    public string OperatorIdentity { get; init; } = string.Empty;
}

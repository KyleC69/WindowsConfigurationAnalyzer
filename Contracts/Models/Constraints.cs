using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;

public class ExecutionConstraints
{
    public bool RunOncePerSession { get; set; }
    public TimeSpan Timeout { get; set; }
    public int RetryCount { get; set; }
}

public class ScoringModule
{
    public int SeverityScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // e.g., Low, Medium, High
}

public class AuditModule
{
    public bool EnableAuditTrail { get; set; }
    public string AuditLogTarget { get; set; } = string.Empty;
}

public class DependencyModule
{
    public List<string> ParentRules { get; set; } = new();
    public List<string> ChildRules { get; set; } = new();
}

public class TaggingModule
{
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public class ResultSchemaModule
{
    public string Format { get; set; } = string.Empty; // JSON, XML, Table
    public bool IncludeTimestamp { get; set; } = true;
}
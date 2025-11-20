using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;

public class WorkflowContract
{
    // Identity & provenance
    public string WorkflowName { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = "1.0";
    public string Author { get; set; }= "Kyle L Crowder";
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // Global parameters (shared context across rules)
    public WorkflowParameters GlobalParameters { get; set; } = new();

    // Rules (composable units)
    
    public IList<RuleContract> Rules { get; set; } = new List<RuleContract>();

    // Grouping & nesting
    public IList<string> ParentWorkflows { get; set; } = new List<string>();
    public IList<string> ChildWorkflows { get; set; } = new List<string>();

    // Execution constraints
    public WorkflowConstraints Constraints { get; set; } = new();

    // Results aggregation
    public List<WorkflowResult> Results { get; set; } = new();
}
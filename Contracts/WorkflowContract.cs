//  Created:  2025/11/23
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   WorkflowContract.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using Newtonsoft.Json;

#endregion



#region Top-level workflow contract

/// <summary>
///     Canonical workflow contract: source of truth for schema generation and execution.
/// </summary>
public class WorkflowContract
{


    /// <summary>
    ///     Optional JSON Schema id pointer for validators.
    ///     Serialized as "$schema".
    /// </summary>
    [JsonProperty("$schema")]
    public string SchemaUri { get; set; } = string.Empty;

    /// <summary>
    ///     Human-readable workflow name.
    ///     Required.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string WorkflowName { get; set; } = string.Empty;

    /// <summary>
    ///     Version of the workflow schema (contract versioning).
    /// </summary>
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>
    ///     Author or maintainer of this workflow.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    ///     Creation timestamp.
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Applicability constraints (OS family, product, versions).
    /// </summary>
    public Applicability Applicability { get; set; } = new();

    /// <summary>
    ///     Workflow-level execution constraints.
    /// </summary>
    public WorkflowConstraints Constraints { get; set; } = new();

    /// <summary>
    ///     Ordered set of rules to evaluate.
    ///     Required.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public List<Rule> Rules { get; set; } = [];

    /// <summary>
    ///     Aggregated results produced after execution.
    /// </summary>
    public List<Result> Results { get; set; } = [];


}



/// <summary>
///     OS applicability envelope.
/// </summary>
public class Applicability
{


    public string OSFamily { get; set; } = string.Empty; // e.g., "Windows"
    public string MinVersion { get; set; } = string.Empty; // e.g., "10.0"
    public string MaxVersion { get; set; } = string.Empty; // e.g., "10.0.22631"
    public string Product { get; set; } = string.Empty; // e.g., "Windows Server 2022"


}



/// <summary>
///     Workflow-level constraints controlling execution behavior.
/// </summary>
public class WorkflowConstraints
{


    /// <summary>
    ///     If true, run rules strictly in sequence; otherwise executor may parallelize.
    /// </summary>
    public bool RunSequentially { get; set; } = true;

    /// <summary>
    ///     If true, stop the workflow on first failure.
    /// </summary>
    public bool StopOnFailure { get; set; } = false;

    /// <summary>
    ///     Optional global timeout (hh:mm:ss).
    /// </summary>
    public string Timeout { get; set; } = string.Empty;


}

#endregion

#region Rule contract and provider parameters

/// <summary>
///     Represents a configuration rule that probes a specific OS provider and evaluates conditions.
/// </summary>
public class Rule
{


    /// <summary>
    ///     Human-readable name of the rule.
    ///     Required.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    ///     The provider indicates which OS subsystem to probe.
    ///     Required.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public ProviderType Provider { get; set; }

    /// <summary>
    ///     Provider-specific parameters (see parameter classes below).
    ///     Required.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public IProviderParameters Parameters { get; set; } = default!;

    /// <summary>
    ///     Condition to evaluate against the probed value.
    ///     Requires Operator and Expected.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public Condition Condition { get; set; } = default!;

    /// <summary>
    ///     Severity score (0–10). Higher indicates more critical issues.
    /// </summary>
    public int Severity { get; set; }

    /// <summary>
    ///     Optional message to display if the rule fails.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     Optional tags for categorization.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    ///     Rule-level execution options (overrides workflow constraints for this rule).
    /// </summary>
    public ExecutionOptions Execution { get; set; } = new();


}



/// <summary>
///     OS subsystem providers that rules can probe.
/// </summary>
public enum ProviderType
{


    Registry,
    FileSystem,
    ACL,
    WMI,
    EventLog,
    Service,
    Process,
    Custom


}



/// <summary>
///     Marker interface for provider-specific parameter contracts.
///     JSON Schema will represent this via oneOf among concrete classes.
/// </summary>
public interface IProviderParameters
{


}



/// <summary>
///     Registry probe parameters.
/// </summary>
public class RegistryParameters : IProviderParameters
{


    /// <summary>
    ///     Registry hive, e.g., HKEY_LOCAL_MACHINE, HKEY_CURRENT_USER.
    /// </summary>
    public string Hive { get; set; } = string.Empty;

    /// <summary>
    ///     Registry key path, e.g., SOFTWARE\\Microsoft\\Windows\\CurrentVersion.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    ///     Value name (for querying a specific value within the key).
    /// </summary>
    public string Key { get; set; } = string.Empty;


}



/// <summary>
///     File system probe parameters.
/// </summary>
public class FileSystemParameters : IProviderParameters
{


    /// <summary>
    ///     File or directory path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    ///     Optional ACL descriptor or policy label (engine-specific).
    /// </summary>
    public string Permissions { get; set; } = string.Empty;

    /// <summary>
    ///     Whether to recurse into subdirectories (when Path is a directory).
    /// </summary>
    public bool Recursive { get; set; } = false;


}



/// <summary>
///     Access Control List probe parameters.
/// </summary>
public class AclParameters : IProviderParameters
{


    /// <summary>
    ///     Target object path (file, folder, registry key, etc.).
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    ///     Identity (user/group) to examine.
    /// </summary>
    public string Identity { get; set; } = string.Empty;

    /// <summary>
    ///     Rights required or being evaluated (e.g., Read, Write, FullControl).
    /// </summary>
    public string Rights { get; set; } = string.Empty;


}



/// <summary>
///     WMI probe parameters.
/// </summary>
public class WmiParameters : IProviderParameters
{


    /// <summary>
    ///     WMI namespace, e.g., root\\cimv2.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    ///     WMI class name.
    /// </summary>
    public string Class { get; set; } = string.Empty;

    /// <summary>
    ///     Property to query on the class.
    /// </summary>
    public string Property { get; set; } = string.Empty;


}



/// <summary>
///     Event Log probe parameters.
/// </summary>
public class EventLogParameters : IProviderParameters
{


    /// <summary>
    ///     Log name, e.g., Application, System.
    /// </summary>
    public string LogName { get; set; } = string.Empty;

    /// <summary>
    ///     Event source (provider name within the log).
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    ///     Specific event ID to match.
    /// </summary>
    public int? EventId { get; set; }

    /// <summary>
    ///     Optional starting point filter (UTC).
    /// </summary>
    public DateTime? Since { get; set; }


}



/// <summary>
///     Windows Service probe parameters.
/// </summary>
public class ServiceParameters : IProviderParameters
{


    /// <summary>
    ///     Service name, e.g., "Spooler".
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    ///     Expected status (e.g., Running, Stopped, Disabled).
    /// </summary>
    public string ExpectedStatus { get; set; } = string.Empty;


}



/// <summary>
///     Process probe parameters.
/// </summary>
public class ProcessParameters : IProviderParameters
{


    /// <summary>
    ///     Process name, e.g., "explorer.exe".
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    ///     Optional expected instance count.
    /// </summary>
    public int? ExpectedCount { get; set; }


}



/// <summary>
///     Custom probe parameters (escape hatch).
/// </summary>
public class CustomParameters : IProviderParameters
{


    /// <summary>
    ///     Arbitrary key-value bag for custom providers.
    /// </summary>
    public Dictionary<string, object> Values { get; set; } = [];


}

#endregion

#region Condition and execution

/// <summary>
///     Supported operators for condition evaluation.
/// </summary>
public enum OperatorType
{


    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    Contains,
    NotContains,
    RegexMatch,
    Exists,
    NotExists


}



/// <summary>
///     Condition object that defines how to evaluate a probed value.
/// </summary>
public class Condition
{


    /// <summary>
    ///     The operator used for comparison.
    ///     Required.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public OperatorType Operator { get; set; }

    /// <summary>
    ///     The expected value to compare against (string, number, boolean, array, object, or null).
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public object Expected { get; set; } = new();


}



/// <summary>
///     Options controlling how the rule executes.
/// </summary>
public class ExecutionOptions
{


    /// <summary>
    ///     Run mode for the rule (default: Independent).
    /// </summary>
    public RunModeType RunMode { get; set; } = RunModeType.Independent;

    /// <summary>
    ///     Optional per-rule timeout in hh:mm:ss format.
    /// </summary>
    public string Timeout { get; set; } = string.Empty;

    /// <summary>
    ///     Whether to stop workflow execution if this rule fails (default: false).
    /// </summary>
    public bool StopOnFailure { get; set; } = false;

    /// <summary>
    ///     Optional list of other rule names this rule depends on.
    /// </summary>
    public List<string> DependsOn { get; set; } = [];


}



/// <summary>
///     Execution modes for rules.
/// </summary>
public enum RunModeType
{


    Independent,
    Sequential


}

#endregion

#region Result contract

/// <summary>
//— Result record produced for each rule after workflow execution.
/// </summary>
public class Result
{


    /// <summary>
    ///     Name of the rule that produced this result.
    /// </summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the rule evaluation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     Message or explanation associated with the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     Severity score recorded at evaluation time.
    /// </summary>
    public int SeverityScore { get; set; }

    /// <summary>
    ///     Timestamp (UTC) when the result was produced.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Schema version used during execution (for provenance).
    /// </summary>
    public string SchemaVersion { get; set; } = string.Empty;

    /// <summary>
    ///     Execution mode the rule ran under.
    /// </summary>
    public string ExecutionMode { get; set; } = "Independent"; // "Independent" or "Sequential"


}

#endregion
//  Created:  2025/11/25
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   WorkflowResultSet.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts;


/// <summary>
///     Result record produced for each rule after workflow execution.
/// </summary>
public class WorkflowResultSet
{



    /// <summary>
    ///     Workflow name that produced this result.
    /// </summary>
    public required string? WorkflowName { get; set; }

    /// <summary>
    ///     Name of the rule that produced this result.
    /// </summary>
    // public required string RuleName { get; set; }

    /// <summary>
    ///     Whether the rule evaluation succeeded. ALL rules must succeed for the workflow to be considered successful.
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    ///     Message or explanation associated with the result.
    /// </summary>
    public required string Message { get; set; } = string.Empty;

    /// <summary>
    ///     Severity score should be computed from the highest severity of any failed rule in the workflow.
    /// </summary>
    public int SeverityScore { get; set; }

    /// <summary>
    ///     Timestamp (UTC) when the result was produced.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;




    public List<RuleResult> Results { get; set; } = new();


    public string SchemaVersion { get; set; } = string.Empty;


}
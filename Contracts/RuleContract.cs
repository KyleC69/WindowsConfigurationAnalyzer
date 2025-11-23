//  Created:  2025/11/17
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   RuleContract.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts;


public class RuleContract
{


    // Identity & provenance
    public string RuleName { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = "1.1";
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string Category { get; set; } = string.Empty; // e.g., Security, Configuration, Performance
    public List<string> Tags { get; set; } = []; // flexible classification

    // Parameters (local scope for this rule)
    public List<RuleParameter> Parameters { get; set; } = [];

    // Linking/navigation to other rules
    public List<string> ParentRules { get; set; } = [];
    public List<string> ChildRules { get; set; } = [];

    // Probe target (where data comes from)
    public ProbeTarget Probe { get; set; } = new();

    // Expression (logic to evaluate)
    public ExpressionDefinition Expression { get; set; } = new();

    // Messaging
    public string SuccessMessage { get; set; } = string.Empty;
    public string FailureMessage { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    // Actions (side effects)
    public List<RuleAction> OnSuccess { get; set; } = [];
    public List<RuleAction> OnFailure { get; set; } = [];

    // Execution constraints
    public ExecutionConstraints Constraints { get; set; } = new();

    // Scoring / weighting
    public int SeverityScore { get; set; } = 0;

    // Audit hooks
    public bool EnableAuditTrail { get; set; } = true;


}
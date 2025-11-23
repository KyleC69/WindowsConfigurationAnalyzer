//  Created:  2025/11/17
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




namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;


public class WorkflowContract
{


    // Identity & provenance
    public string WorkflowName { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = "1.0";
    public string Author { get; set; } = "Kyle L Crowder";
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // Global parameters (shared context across rules)
    public WorkflowParameters GlobalParameters { get; set; } = new();

    // Rules (composable units)

    public IList<RuleContract> Rules { get; set; } = [];

    // Grouping & nesting
    public IList<string> ParentWorkflows { get; set; } = [];
    public IList<string> ChildWorkflows { get; set; } = [];

    // Execution constraints
    public WorkflowConstraints Constraints { get; set; } = new();

    // Results aggregation
    public List<WorkflowResult> Results { get; set; } = [];


}
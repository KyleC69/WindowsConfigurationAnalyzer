//  Created:  2025/11/17
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




namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;


public class WorkflowResultSet
{


    public string WorkflowName { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartedOn { get; set; }
    public DateTime CompletedOn { get; set; }
    public List<RuleResult> Results { get; set; } = [];


}
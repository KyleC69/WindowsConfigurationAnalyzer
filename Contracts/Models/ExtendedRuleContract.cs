//  Created:  2025/11/17
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   ExtendedRuleContract.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;


internal class ExtendedRuleContract
{


    // Extensions (optional)
    public ExecutionConstraints? Constraints { get; set; }
    public ScoringModule? Scoring { get; set; }
    public AuditModule? Audit { get; set; }
    public DependencyModule? Dependencies { get; set; }
    public TaggingModule? Tags { get; set; }
    public ResultSchemaModule? ResultSchema { get; set; }


}
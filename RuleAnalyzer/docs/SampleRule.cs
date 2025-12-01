//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   SampleRule.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.docs;


public class SampleRule
{


    /*

    private RuleContract SampleRuleMethod()
    {
        RuleContract registryRule = new()
        {
            RuleName = "CheckProfileRegistryKey",
            Author = "Kyle",
            Category = "Configuration",
            Tags = ["Registry", "Profile"],
            Parameters =
            [
                new() { Name = "RegistryProfileKey", Type = ParameterType.String }
            ],
            Probe = new ProbeTarget
            {
                Subsystem = SubsystemType.Registry,
                Location = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList",
                Pattern = @"S-1-5-21.*"
            },
            Expression = new ExpressionDefinition
            {
                ExpressionType = ExpressionType.FunctionCall,
                ExpressionText = "Registry.Exists(RegistryProfileKey)"
            },
            SuccessMessage = "Profile registry key exists.",
            FailureMessage = "Profile registry key missing.",
            OnSuccess =
            [
                new() { ActionType = ActionType.Log, Target = "audit.log" }
            ],
            OnFailure =
            [
                new() { ActionType = ActionType.Notify, Target = "admin@domain.com" }
            ],
            Constraints = new ExecutionConstraints
            {
                RunOncePerSession = true,
                Timeout = TimeSpan.FromSeconds(10)
            },
            SeverityScore = 5,
            EnableAuditTrail = true
        };

        return registryRule;
    */


}
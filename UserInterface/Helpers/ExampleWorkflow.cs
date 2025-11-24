//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   ExampleWorkflow.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


public static class ExampleWorkflow
{


    public static WorkflowContract GetSampleWorkflow()
    {
        return new WorkflowContract
        {
            WorkflowName = "ProfileValidationWorkflow",
            Author = "Kyle",

            GlobalParameters = new WorkflowParameters
            {
                ProviderName = "ProfileProvider",
                ManifestPath = @"C:\Manifests\profile.man",
                ProbeTimestamp = DateTime.UtcNow
            },

            Rules =
            [
                new()
                {
                    RuleName = "CheckProfileRegistryKey",
                    Parameters =
                    [
                        new()
                        {
                            Name = "RegistryProfileKey",
                            Type = ParameterType.String
                        }
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
                    FailureMessage = "Profile registry key missing."
                },

                new()
                {
                    RuleName = "CheckLocalProfilePath",
                    Parameters =
                    [
                        new()
                        {
                            Name = "ProfilePath",
                            Type = ParameterType.String
                        }
                    ],
                    Probe = new ProbeTarget
                    {
                        Subsystem = SubsystemType.FileSystem,
                        Location = @"C:\Users",
                        Pattern = @"*"
                    },
                    Expression = new ExpressionDefinition
                    {
                        ExpressionType = ExpressionType.FunctionCall,
                        ExpressionText = "Directory.Exists(ProfilePath)"
                    },
                    SuccessMessage = "Local profile path exists.",
                    FailureMessage = "Local profile path missing."
                }
            ],

            Constraints = new WorkflowConstraints
            {
                RunSequentially = true,
                StopOnFailure = false
            }
        };
    }


}
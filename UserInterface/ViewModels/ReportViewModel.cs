//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   ReportViewModel.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KC.WindowsConfigurationAnalyzer.Contracts.Models;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

using Newtonsoft.Json;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;


public partial class ReportViewModel : ObservableRecipient
{


    private static readonly string ProjectDir = App.ProjectDir ?? @"D:\\Solutions\\KC.WindowsConfigurationAnalyzer";
    public string RULESTOREFOLDER = Path.Combine(ProjectDir, "RulesEngineStore");





    public ReportViewModel()
    {
        RunRulesCommand = new AsyncRelayCommand(ExecuteRunRulesAsync);
        LoadRulesCollection();
    }





    public ObservableCollection<RuleItem> RuleListCollection { get; } = [];
    public ObservableCollection<WorkflowContract> WorkFlows { get; } = [];
    public AsyncRelayCommand RunRulesCommand { get; }





    private void LoadRulesCollection()
    {
        //Read all the json files in the rule store and populate the Rule collection
        WorkflowContract sample = ExampleWorkflow.GetSampleWorkflow();

        try
        {
            string[] rulesFiles = Directory.GetFiles(RULESTOREFOLDER, "*.json", SearchOption.AllDirectories);

            foreach (string file in rulesFiles)
            {
                if (File.Exists(file))
                {
                    string ruleFileContent = File.ReadAllText(file);
                    WorkflowContract? ruleJson = JsonConvert.DeserializeObject<WorkflowContract>(ruleFileContent);
                    if (ruleJson != null) WorkFlows.Add(ruleJson);


                    // For testing purposes only- write out a sample workflow file
                    // var stringjson= JsonSerializer.Serialize<WorkflowContract>(sample, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    //  File.WriteAllText(Path.Combine(RULESTOREFOLDER, "sampleworkflow.json"), stringjson);
                }
            }
        }
        catch (Exception e)
        {
            ActivityLogger.Log("ERR", $"Failure attempting to load workflows from file:: {e.Message}", "ReportViewModel::LoadRulesCollection");
        }
    }





    private async Task ExecuteRunRulesAsync()
    {
        /*
             var options = new WorkflowConcurrentExecutor.RunOptions { MaxDegreeOfParallelism = 4 };


             var executor = new WorkflowConcurrentExecutor(options);

             // build workflows and run
             var reports = await executor.RunWorkflowsAsync(myWorkflows, CancellationToken.None);

             // traceSink is disposed (flushed) at the end of the using
             */
    }


}



public record RuleItem
{


    public string RuleName { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = false;

    //For ease of loading rules from file- Not intended to be bound to UI
    public string FileName { get; set; } = string.Empty;


}
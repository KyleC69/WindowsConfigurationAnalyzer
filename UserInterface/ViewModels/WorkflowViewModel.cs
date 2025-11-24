//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   WorkflowViewModel.cs
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

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;


public partial class WorkflowViewModel : ObservableRecipient
{


    private static readonly string ProjectDir = App.ProjectDir ?? @"D:\\Solutions\\KC.WindowsConfigurationAnalyzer";
    public string RULESTOREFOLDER = Path.Combine(ProjectDir, "RulesEngineStore");





    public WorkflowViewModel()
    {
        RunRulesCommand = new AsyncRelayCommand(ExecuteRunRulesAsync);
    }





    public ObservableCollection<RuleItem> RuleListCollection { get; } = [];
    public ObservableCollection<WorkflowContract> WorkFlows { get; } = [];
    public AsyncRelayCommand RunRulesCommand { get; }










    /// <summary>
    ///     Metho starts the rules engine and runs all the rules that exist in the RulesStore folder.
    /// </summary>
    /// <returns></returns>
    private async Task ExecuteRunRulesAsync()
    {
        IActivityLogger logger = App.GetService<IActivityLogger>();
        InitializeEngine engine = new(logger);
        var results = await engine.StartRulesEngine();
    }


}



public record RuleItem
{


    public string RuleName { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = false;

    //For ease of loading rules from file- Not intended to be bound to UI
    public string FileName { get; set; } = string.Empty;


}
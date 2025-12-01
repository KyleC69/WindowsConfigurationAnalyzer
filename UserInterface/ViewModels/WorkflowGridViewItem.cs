//  Created:  2025/12/01
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   WorkflowGridViewItem.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.ComponentModel;
using System.Runtime.CompilerServices;

using KC.WindowsConfigurationAnalyzer.Contracts;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;


public partial class WorkflowGridViewItem : WorkflowContract, INotifyPropertyChanged, INotifyPropertyChanging
{


    private bool _isSelected;
    private DateTime _lastRun;
    private string? _lastRunResult;






    public WorkflowGridViewItem(WorkflowContract workflowContract)
    {
        // Copy all required members explicitly to satisfy CS9035
        SchemaRef = workflowContract.SchemaRef;
        WorkflowName = workflowContract.WorkflowName;
        WorkflowDescription = workflowContract.WorkflowDescription;
        Author = workflowContract.Author;
        SchemaVersion = workflowContract.SchemaVersion;
        CreatedOn = workflowContract.CreatedOn;
        Applicability = workflowContract.Applicability;
        Constraints = workflowContract.Constraints;
        Rules = workflowContract.Rules;
        Results = workflowContract.Results;
    }






    public int RuleCount
    {
        get => Rules?.Count ?? 0;
    }


    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }


    public DateTime LastRun
    {
        get => _lastRun;
        set => SetField(ref _lastRun, value);
    }


    public string LastRunResult
    {
        get => _lastRunResult;
        set => SetField(ref _lastRunResult, value);
    }


    public event PropertyChangedEventHandler? PropertyChanged;





    /// <summary>Occurs when a property value is changing.</summary>
    public event PropertyChangingEventHandler? PropertyChanging;





    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }





    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }


}
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




using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Etw;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

using Microsoft.UI.Dispatching;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;


public partial class WorkflowViewModel : ObservableRecipient
{


    private readonly CancellationTokenSource _cancellationTokenSource = new();


    private readonly IList<string> _jsonFiles = new List<string>();
    private bool _isRunning;

    [ObservableProperty] public List<WorkflowSerializationError> _jsonErrors = [];
    private string _name = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ValidateRulesCommand))]
    private WorkflowContract? _selectedWorkflow;

    private object _validateRulesCommand = new();





    public WorkflowViewModel()
    {
        RunRulesCommand = new AsyncRelayCommand(
            () => ExecuteRunRulesAsync(_cancellationTokenSource.Token),
            () => !IsRunning);

        ValidateRulesCommand = new AsyncRelayCommand(
            () => ExecuteValidateRulesAsync(_cancellationTokenSource.Token),
            () => !IsRunning);

        RefreshListCommand = new AsyncRelayCommand(
            () => ExecuteRefreshListAsync(_cancellationTokenSource.Token),
            () => !IsRunning);

        CancelCommand = new RelayCommand(CancelOperations, () => IsRunning);

        LoadWorkflowsFromFiles();
    }





    private string RulesStore
    {
        get => App.RulesStore!;
    }


    // List of json Files names in the RulesStore folder
    public ObservableCollection<WorkflowGridViewItem> Workflows { get; } = new();


    public ObservableCollection<string> JsonFileNames
    {
        get => new(_jsonFiles);
    }


    public AsyncRelayCommand RunRulesCommand { get; }
    public AsyncRelayCommand ValidateRulesCommand { get; }
    public AsyncRelayCommand RefreshListCommand { get; }


    public string SelectedJsonFile
    {
        get => _name;
        set => SetProperty(ref _name, value); // Using SetProperty
    }


    public RelayCommand CancelCommand { get; }
    public DispatcherQueue? dispatcher { get; set; }


    public bool IsRunning
    {
        get => _isRunning;

        set
        {
            _isRunning = value;
            CancelCommand.NotifyCanExecuteChanged();
            RunRulesCommand.NotifyCanExecuteChanged();
            ValidateRulesCommand.NotifyCanExecuteChanged();
            RefreshListCommand.NotifyCanExecuteChanged();
        }
    }





    private void CancelOperations()
    {
        _cancellationTokenSource.Cancel();
    }








    private async Task ExecuteRefreshListAsync(CancellationToken token)
    {
        LoadWorkflowsFromFiles();
    }





    /// <summary>
    ///     Validates the selected JSON file against a predefined JSON schema.
    /// </summary>
    /// <param name="arg">
    ///     A <see cref="CancellationToken" /> that can be used to cancel the validation operation.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous validation operation.
    /// </returns>
    /// <remarks>
    ///     This method reads the selected JSON file and validates its structure and content
    ///     against a schema defined in the "workflow.schema.json" file. If validation errors
    ///     are found, they are logged to the console.
    /// </remarks>
    private Task ExecuteValidateRulesAsync(CancellationToken arg)
    {
        //TODO: Normalize hardcoded paths - retrieve from system settings source TBD
        var jsonString = File.ReadAllText(Path.Combine(RulesStore, SelectedJsonFile));
        var schemaJson = File.ReadAllText(Path.Combine(@"D:\Solutions\Repos", @"kylec69.github.io\kylec69.github.io", "schemas", "workflow-schema-1.0.0.json"));
        JSchema schema = JSchema.Parse(schemaJson);
        var reader = new JsonTextReader(new StringReader(jsonString));
        IList<string> messages = new List<string>();

        try
        {
            var validatingReader = new JSchemaValidatingReader(reader);
            validatingReader.Schema = JSchema.Parse(schemaJson);

            validatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);

            var serializer = new JsonSerializer
            {
                Converters = { new RuleConverter() } // your discriminator converter
            };

            var workflow = serializer.Deserialize<WorkflowContract>(validatingReader);

            if (workflow is null || messages.Count > 0)
                ActivityLogger.Log("WAR", $"Validation completed. Found {messages.Count} issues.", "WorkflowViewModel");
            else
                ActivityLogger.Log("WAR", $"Validation completed. Found {messages.Count} issues.", "WorkflowViewModel");
        }
        catch (Exception ex)
        {
            WCAEventSource.Log.ExceptionWarning(ex.Message);
            ActivityLogger.Log("ERR", $"Validation failed exception thrown. Found {messages.Count} issues attempting to validate.", "WorkflowViewModel");
        }

        var isValid = messages.Count == 0;

        return Task.FromResult(messages);
    }





    /// <summary>
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private WorkflowContract? ParseJsonIntoObject(string filePath)
    {
        ActivityLogger.Log("INF", $"Parsing JSON file into WorkflowContract object: {filePath}", "WorkflowViewModel");

        var jsonString = File.ReadAllText(filePath);

        if (jsonString == null) return null;

        try
        {
            return JsonConvert.DeserializeObject<WorkflowContract>(jsonString, new JsonSerializerSettings { Converters = [new RuleConverter()] });
        }
        catch (JsonSerializationException ex)
        {
            JsonErrors.Add(new WorkflowSerializationError(Path.GetFileName(filePath), ex.Message));
            ActivityLogger.Log("ERR", ex.Message, $"Workflow path: {filePath}");
            WCAEventSource.Log.ReaderError(EventSource.CurrentThreadActivityId.ToString(), "Failed to parse JSON into WorkflowContract.", ex.Message, "");
        }

        return null;
    }





    private void LoadWorkflowsFromFiles()
    {
        if (!Directory.Exists(RulesStore)) throw new ArgumentException("Rules Store folder does not exist.");

        var paths = Directory.GetFiles(RulesStore, "*.json");

        foreach (var nm in paths)
        {
            _jsonFiles.Add(Path.GetFileNameWithoutExtension(nm));
            WorkflowContract? wf = ParseJsonIntoObject(nm);
            if (wf != null) Workflows.Add(new WorkflowGridViewItem(wf));
        }
    }





    /// <summary>
    ///     Method starts the selected workflows through the rules engine .
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task ExecuteRunRulesAsync(CancellationToken token)
    {
        IsRunning = true;
        //  await Task.Delay(100000); // Simulate long-running operation


        WCAEventSource.Log.ActionStart("ExecuteRunRulesAsync, Starting to run workflows through the engine.");
        var logger = App.GetService<IActivityLogger>();


        List<WorkflowGridViewItem> work = Workflows.Where(wf => wf.IsSelected).ToList();
        List<WorkflowContract> flow = work.Select(wf => (WorkflowContract)wf).ToList();

        if (work.Count == 0)
        {
            ActivityLogger.Log("WRN", "No workflows selected to run.", "WorkflowViewModel");

            return;
        }

        try
        {
            InitializeEngine engine = new(logger, flow);

            ICollection<WorkflowResultSet> results = await engine.StartRulesEngine(token);

            // Process results as needed
            UpdateGridData(results);
        }
        catch (OperationCanceledException oc)
        {
            WCAEventSource.Log.ActionStop("ExecuteRunRulesAsync, Workflow run operation was canceled.");
        }
        finally
        {
            IsRunning = false;
        }
    }





    private void UpdateGridData(ICollection<WorkflowResultSet> results)
    {
        ActivityLogger.Log("INF", "Starting Grid update method.", "UpdateGridItem");

        if (results.Count == 0) return;

        void doUpdate()
        {
            foreach (WorkflowResultSet result in results)
            {
                WorkflowGridViewItem? workflowItem = Workflows.FirstOrDefault(wf => wf.WorkflowName == result.WorkflowName);


                if (workflowItem != null)
                {
                    Debug.WriteLine($"Updating workflow item: {workflowItem.WorkflowName} with result: {result.Success} at {result.Timestamp}");

                    workflowItem.LastRun = result.Timestamp;
                    workflowItem.LastRunResult = result.Success ? "Success" : "Failure";
                    workflowItem.Results = result.Results;
                }
                else
                {
                    Debug.WriteLine("Updating workflow item failed.");
                    ActivityLogger.Log("ERR", "No workloads found matching the result flow.", "UdateGridData");
                }
            }
        }


        if (dispatcher == null || dispatcher.HasThreadAccess)
            doUpdate();
        else
            // Use Invoke to ensure UI is updated synchronously; change to InvokeAsync if you prefer non-blocking.
            dispatcher.TryEnqueue(doUpdate);
    }


}
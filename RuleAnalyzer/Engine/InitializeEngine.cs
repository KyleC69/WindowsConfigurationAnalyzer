//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   InitializeEngine.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

#region

using System.Reflection;
using System.Text.Json;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Etw;

using Newtonsoft.Json;

using NJsonSchema.Validation;

#endregion

#endregion





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


public class InitializeEngine
{


    private readonly ApplicabilityEvaluator _evaluator;



    private readonly IActivityLogger _logger;
    private readonly SchemaValidator _validator;
    private WorkflowOrchestrator _orchestrator = null!;





    public InitializeEngine(IActivityLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = new SchemaValidator(
            "https://kylec69.github.io/schemas/workflow.schema.json",
            "https://kylec69.github.io/schemas/rule.schema.json", logger);
        _evaluator = new ApplicabilityEvaluator();
        WCAEventSource.Log.ActionStart("InitializeEngine:: Starting to Initialize Rules Engine.");
    }





    public static string? ProjectDir => Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "ProjectDirectory")?.Value;





    public WorkflowOrchestrator GetOrchestrator()
    {
        List<IProbe> probes =
        [
            // Add probe implementations here
            // new RegistryReader(_logger),
            // new CimReader(_logger),
            // new FileSystemReader(_logger),
            new EnvironmentReader(_logger),
            new CimReader(_logger),
            new RegistryReader(_logger),
            new FileSystemReader(_logger),
            new AclReader(_logger)
        ];



        _orchestrator = new WorkflowOrchestrator(probes);

        return _orchestrator;
    }









    public async Task<ICollection<WorkflowResultSet>> StartRulesEngine()
    {
        WCAEventSource.Log.ActionStart("InitializeEngine:: Starting Rules Engine.");
        _logger.Log("INF", "Starting Rules Engine", "InitializeEngine");


        WCAEventSource.Log.ActionStop("InitializeEngine:: Rules Engine Initialization Complete.");

        return await LoadRulesAsync();
    }










    public async Task<ICollection<WorkflowResultSet>> LoadRulesAsync()
    {
        //TODO: Remove hardcoded path
        string? SolutionDir = Directory.GetParent(ProjectDir!)?.FullName;
        string RuleStore = Path.Combine(SolutionDir!, "RulesEngineStore");
        string[] rulesFiles = Directory.GetFiles(RuleStore, "*.json", SearchOption.AllDirectories);

        ICollection<WorkflowResultSet> allResults = [];

        if (rulesFiles.Length == 0)
        {
            await Task.FromResult(allResults);
        }

        string[] rulesJson = [];



        foreach (string i in rulesFiles)
        {
            string json = await File.ReadAllTextAsync(i);


            JsonDocument ruleDoc = JsonDocument.Parse(json);
            //Validate the schema first, if it isn't right we don't want to proceed
            ValidateSchema(ruleDoc, i, out bool IsValid);



            ValidateOperatingSystem(ruleDoc, i, out bool isApplicable);

            if (!IsValid || !isApplicable)
            {
                continue;
            }

            Workflow workflow = JsonConvert.DeserializeObject<Workflow>(json) ?? throw new InvalidOperationException();
            if (workflow != null)
            {
                allResults.Add(await _orchestrator.RunAsync(workflow));
            }
        }

        return allResults;
    }





    private void ValidateSchema(JsonDocument ruleDoc, string i, out bool IsValid)
    {
        bool isValid = false;
        try
        {
            IsValid = _validator.ValidateWorkflow(ruleDoc, out ICollection<ValidationError> results);
            _logger.Log("INF", $"Validating rule file {i}. Result: {IsValid}", "InitializeEngine");
            if (!IsValid)
            {
                _logger.Log("ERR", $"Rule file {i} failed schema validation: {results}", "InitializeEngine");
                WCAEventSource.Log.RulesEngineWarning($"Rule file {i} failed schema validation: {results} Skipping rule. Check rule and try again.");
            }

            isValid = IsValid;
        }
        catch (Exception e)
        {
            WCAEventSource.Log.RulesEngineError($"Exception thrown while validating rule file {i}: {e.Message}");
            _logger.Log("ERR", $"Error validating rule file {i}: {e.Message}", "InitializeEngine");
            IsValid = false; // Ensure out parameter is always assigned
        }
    }





    private void ValidateOperatingSystem(JsonDocument ruleDoc, string i, out bool isApplicable)
    {
        isApplicable = false;
        try
        {
            isApplicable = _evaluator.IsApplicable(ruleDoc.RootElement.GetProperty("Applicability"));
            if (!isApplicable)
            {
                _logger.Log("INF", $"Rule file {i} is not applicable to this machine. Skipping.", "InitializeEngine");
                WCAEventSource.Log.RulesEngineWarning($"Rule file {i} is not applicable to this machine. Skipping.");
            }

            isApplicable = isApplicable;
        }
        catch (Exception e)
        {
            _logger.Log("ERR", $"Error evaluating applicability for rule file {i}: {e.Message}", "InitializeEngine");
            WCAEventSource.Log.RulesEngineError($"Exception thrown while evaluating applicability for rule file {i}: {e.Message}");
        }
    }


}
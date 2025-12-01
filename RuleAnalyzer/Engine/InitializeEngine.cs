//  Created:  2025/11/24
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




using System.Text.Json;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Etw;





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


public class InitializeEngine
{


    private readonly ApplicabilityEvaluator _evaluator;
    private readonly IActivityLogger _logger;
    private readonly WorkflowOrchestrator _orchestrator = null!;
    private readonly List<WorkflowContract> _workflows = null!;





    public InitializeEngine(IActivityLogger logger, List<WorkflowContract> flowList)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _evaluator = new ApplicabilityEvaluator();
        _workflows = flowList ?? throw new ArgumentNullException(nameof(flowList));
        _orchestrator = GetOrchestrator();
        WCAEventSource.Log.ActionStart("InitializeEngine:: Starting to Initialize Rules Engine.");
    }





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

        return new WorkflowOrchestrator(probes);
    }





    public async Task<ICollection<WorkflowResultSet>> StartRulesEngine(CancellationToken token)
    {
        List<WorkflowResultSet> allResults = new();
        token.ThrowIfCancellationRequested();

        // Process each workflow in parallel
        await Parallel.ForEachAsync(_workflows, async (workflow, ct) =>
        {
            var result = await _orchestrator.RunAsync(workflow, ct);
            allResults.Add(result);
        });

        return allResults;
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
        }
        catch (Exception e)
        {
            _logger.Log("ERR", $"Error evaluating applicability for rule file {i}: {e.Message}", "InitializeEngine");
            WCAEventSource.Log.RulesEngineError($"Exception thrown while evaluating applicability for rule file {i}: {e.Message}");
        }
    }


}
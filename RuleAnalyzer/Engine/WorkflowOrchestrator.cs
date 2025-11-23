//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   WorkflowOrchestrator.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;

#endregion





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


public class WorkflowOrchestrator
{


    private readonly IDictionary<string, IProbe> _probes;





    public WorkflowOrchestrator(IEnumerable<IProbe> probes)
    {
        _probes = probes.ToDictionary(p => p.Provider, StringComparer.OrdinalIgnoreCase);
    }





    public async Task<WorkflowResultSet> RunAsync(
        WorkflowContract workflow,
        CancellationToken externalToken = default)
    {
        WorkflowResultSet resultSet = new()
        {
            WorkflowName = workflow.WorkflowName,
            SchemaVersion = workflow.SchemaVersion,
            Description = $"Execution of workflow {workflow.WorkflowName}",
            StartedOn = DateTime.UtcNow
        };
        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        /*
            if (workflow.Constraints?.Timeout != default)
                        cts.CancelAfter(workflow.Constraints.Timeout);
            */
        List<RuleResult> results = [];

        if (workflow.Constraints.RunSequentially)
        {
            foreach (RuleContract rule in workflow.Rules)
            {
                RuleResult res = await ExecuteRuleAsync(rule, cts.Token);
                results.Add(res);

                if (!res.Success && workflow.Constraints.StopOnFailure)
                {
                    break;
                }
            }
        }
        else
        {
            // Parallel execution with dependency awareness
            HashSet<string> remaining = [.. workflow.Rules.Select(r => r.RuleName)];
            Dictionary<string, RuleContract> ruleLookup = workflow.Rules.ToDictionary(r => r.RuleName);
            /*
                while (remaining.Count > 0)
                {
                    var runnable = remaining
                        .Select(name => ruleLookup[name])
                        .Where(rc =>
                        {
                            var deps = rc.Execution?.DependsOn ?? Array.Empty<string>();
                            return deps.All(d => results.Any(r => r.RuleName == d && r.Success));
                        })
                        .ToList();

                    if (runnable.Count == 0) break; // deadlock

                    var tasks = runnable.Select(async rc =>
                    {
                        var res = await ExecuteRuleAsync(rc, cts.Token);
                        lock (results) results.Add(res);
                        remaining.Remove(rc.RuleName);
                    });

                    await Task.WhenAll(tasks);
                }
        */
        }

        resultSet.Results = results;
        resultSet.CompletedOn = DateTime.UtcNow;

        return resultSet;
    }





    private async Task<RuleResult> ExecuteRuleAsync(RuleContract rule, CancellationToken token)
    {
        RuleResult result = new()
        {
            RuleName = rule.RuleName,
            Timestamp = DateTime.UtcNow,
            SchemaVersion = "1.0"
        };

        /*
            try
            {
                if (!_probes.TryGetValue(rule.Provider, out var probe))
                {
                    result.Success = false;
                    result.Message = $"No probe registered for provider {rule.Provider}";
                    result.SeverityScore = 10;
                    return result;
                }

                var probeResult = probe.Execute(rule.Parameters);

                if (!probeResult.ProbeSuccess)
                {
                    result.Success = false;
                    result.Message = probeResult.Message;
                    result.SeverityScore = 9;
                    return result;
                }

                // Evaluate condition
                bool conditionMet = ConditionEvaluator.Evaluate(
                    probeResult.Value,
                    rule.Condition.Operator,
                    rule.Condition.Expected);

                result.Success = conditionMet;
                result.Message = conditionMet
                    ? rule.Message
                    : $"Condition failed: expected {rule.Condition.Operator} {rule.Condition.Expected}, got {probeResult.Value}";
                result.SeverityScore = rule.Severity;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error executing rule: {ex.Message}";
                result.SeverityScore = 10;
            }

        */
        return await Task.FromResult(result);
    }


}
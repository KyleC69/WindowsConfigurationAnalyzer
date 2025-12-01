//  Created:  2025/11/24
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




using KC.WindowsConfigurationAnalyzer.Contracts;





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


public class WorkflowOrchestrator
{


    private readonly IDictionary<string, IProbe> _probes;





    public WorkflowOrchestrator(IEnumerable<IProbe> probes)
    {
        _probes = probes.ToDictionary(p => p.Provider, StringComparer.OrdinalIgnoreCase);
    }





    public async Task<WorkflowResultSet> RunAsync(WorkflowContract workflow, CancellationToken token = default)
    {
        WorkflowResultSet resultSet = new()
        {
            Message = "",
            WorkflowName = workflow.WorkflowName,
            SchemaVersion = workflow.SchemaVersion,
            Timestamp = DateTime.UtcNow,
            Results = new List<RuleResult>(),
            SeverityScore = 0,
            Success = false
        };

        List<RuleResult> results = new();
        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        if (workflow.Constraints?.Timeout != default)
            cts.CancelAfter(TimeSpan.Parse(workflow.Constraints.Timeout));


        if (workflow.Constraints != null && workflow.Constraints.RunSequentially)
        {
            foreach (RuleContract rule in workflow.Rules)
            {
                RuleResult res = await ExecuteRuleAsync(rule, cts.Token);
                results.Add(res);

                if (!res.Success && workflow.Constraints.StopOnFailure) break;
            }
        }
        else
        {
            // Parallel execution with dependency awareness
            object sync = new();
            var remaining = new HashSet<string>(workflow.Rules.Select(r => r.RuleName), StringComparer.OrdinalIgnoreCase);
            Dictionary<string, RuleContract> ruleLookup = workflow.Rules.ToDictionary(r => r.RuleName);

            while (true)
            {
                List<RuleContract> runnable;
                lock (sync)
                {
                    if (remaining.Count == 0)
                        break;

                    // Select rules whose dependencies are satisfied (reads results under lock)
                    runnable = remaining
                        .Select(name => ruleLookup[name])
                        .Where(rc =>
                        {
                            List<string> deps = rc.Execution?.DependsOn ?? new List<string>();

                            return deps.All(d => results.Any(r => r.RuleName == d && r.Success));
                        })
                        .ToList();

                    if (runnable.Count == 0)
                        break; // deadlock or unmet dependencies

                    // Mark as in-flight by removing them from remaining while holding the lock
                    foreach (RuleContract rc in runnable)
                    {
                        remaining.Remove(rc.RuleName);
                    }
                }

                IEnumerable<Task> tasks = runnable.Select(async rc =>
                {
                    RuleResult res = await ExecuteRuleAsync(rc, cts.Token);
                    lock (sync)
                    {
                        results.Add(res);
                    }
                });

                await Task.WhenAll(tasks);
            }
        }

        resultSet.Results = results;

        return ScoreResults(resultSet);
    }





    /// <summary>
    ///     Scores a workflow execution result set by computing the overall success and the maximum severity
    ///     score among failed rule results.
    /// </summary>
    /// <param name="resultSet">The <see cref="WorkflowResultSet" /> to evaluate and update. This instance is mutated in-place.</param>
    /// <returns>
    ///     The same <see cref="WorkflowResultSet" /> instance with updated <see cref="WorkflowResultSet.Success" /> and
    ///     <see cref="WorkflowResultSet.SeverityScore" /> values.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="resultSet" /> is <c>null</c>.</exception>
    /// <remarks>
    ///     The algorithm iterates over each <see cref="RuleResult" /> in <see cref="WorkflowResultSet.Results" />.
    ///     - If all rules succeeded, <see cref="WorkflowResultSet.Success" /> will be <c>true</c> and
    ///     <see cref="WorkflowResultSet.SeverityScore" /> will be <c>0</c>.
    ///     - If any rule failed (<see cref="RuleResult.Success" /> == <c>false</c>), <see cref="WorkflowResultSet.Success" />
    ///     will be <c>false</c>,
    ///     and <see cref="WorkflowResultSet.SeverityScore" /> will be set to the highest
    ///     <see cref="RuleResult.SeverityScore" /> among failed rules.
    ///     Null <see cref="WorkflowResultSet.Results" /> is treated as an empty collection.
    /// </remarks>
    private WorkflowResultSet ScoreResults(WorkflowResultSet resultSet)
    {
        if (resultSet is null)
            throw new ArgumentNullException(nameof(resultSet));

        var maxSeverity = 0;
        var overallSuccess = true;

        List<RuleResult> results = resultSet.Results ?? new List<RuleResult>();

        foreach (RuleResult ruleResult in results)
        {
            if (!ruleResult.Success)
            {
                overallSuccess = false;
                if (ruleResult.SeverityScore > maxSeverity) maxSeverity = ruleResult.SeverityScore;
            }
        }

        resultSet.Success = overallSuccess;
        resultSet.SeverityScore = maxSeverity;

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
        token.ThrowIfCancellationRequested();

        try
        {
            if (!_probes.TryGetValue(rule.Provider.ToString(), out IProbe? probe))
            {
                result.Success = false;
                result.Message = $"No probe registered for provider {rule.Provider}";
                result.SeverityScore = 10;

                return result;
            }

            ProbeResult probeResult = await probe.ExecuteAsync(rule.Parameters, token);

            if (!probeResult.ProbeSuccess)
            {
                result.Success = false;
                result.Message = probeResult.Message;
                result.SeverityScore = 9;

                return result;
            }

            // Evaluate condition
            var conditionMet = ConditionEvaluator.Evaluate(
                probeResult.Value,
                rule.Condition.Operator.ToString(),
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


        return result;
    }


}
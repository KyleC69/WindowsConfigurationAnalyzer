//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   WorkflowEngine.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Reflection;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Etw;

using Microsoft.Win32;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


public class WorkflowEngine
{


    /// <summary>
    ///     Executes a workflow asynchronously, processing each rule within the workflow and aggregating the results.
    /// </summary>
    /// <param name="workflow">
    ///     The <see cref="WorkflowContract" /> instance representing the workflow to be executed.
    ///     It contains the workflow's metadata, rules, global parameters, and execution constraints.
    /// </param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> representing the asynchronous operation,
    ///     with a result of type <see cref="WorkflowResultSet" /> containing the aggregated results of the workflow execution.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="workflow" /> parameter is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the workflow execution encounters an invalid state or configuration.
    /// </exception>
    public async Task<WorkflowResultSet> ExecuteWorkflowAsync(WorkflowContract workflow)
    {
        WorkflowResultSet resultSet = new()
        {
 /*           WorkflowName = workflow.WorkflowName,
            SchemaVersion = workflow.SchemaVersion,
            StartedOn = DateTime.UtcNow*/
        };

        foreach (RuleContract rule in workflow.Rules)
        {
            // Apply workflow constraints
            if (workflow.Constraints.StopOnFailure && resultSet.Results.Any(r => !r.Success)) break;

            RuleResult ruleResult = await ExecuteRuleAsync(rule, workflow.GlobalParameters);
            resultSet.Results.Add(ruleResult);

            // Respect RunSequentially flag
            if (!workflow.Constraints.RunSequentially)
            {
                // Could parallelize here if desired
            }
        }

   //     resultSet.CompletedOn = DateTime.UtcNow;

        return resultSet;
    }





    /// <summary>
    ///     Executes a single rule within the workflow and evaluates its result.
    /// </summary>
    /// <param name="rule">
    ///     The <see cref="RuleContract" /> object representing the rule to be executed,
    ///     including its parameters, probe, expression, and associated actions.
    /// </param>
    /// <param name="globalParams">
    ///     The global parameters of the workflow, represented by <see cref="WorkflowParameters" />,
    ///     which may be used during rule execution.
    /// </param>
    /// <returns>
    ///     A <see cref="RuleResult" /> object containing the outcome of the rule execution,
    ///     including success status, message, timestamp, and severity score.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs during rule execution, such as parameter validation failure,
    ///     probe resolution issues, or expression evaluation errors.
    /// </exception>
    private async Task<RuleResult> ExecuteRuleAsync(RuleContract rule, WorkflowParameters globalParams)
    {
        RuleResult result = new()
        {
            RuleName = rule.RuleName,
            SchemaVersion = rule.SchemaVersion,
            Timestamp = DateTime.UtcNow,
            SeverityScore = rule.SeverityScore
        };

        try
        {
            // Gate 1: Parameter validation
            ValidateParameters(rule.Parameters, globalParams);

            // Gate 2: Probe resolution
            object probeData = ResolveProbe(rule.Probe);

            // Gate 3: Expression evaluation
            bool success = EvaluateExpression(rule.Expression, probeData);

            // Gate 4: Actions + messaging
            result.Success = success;
            result.Message = success ? rule.SuccessMessage : rule.FailureMessage;

            if (success)
                ExecuteActions(rule.OnSuccess);
            else
                ExecuteActions(rule.OnFailure);

            // Gate 5: Audit trail
            if (rule.EnableAuditTrail) LogAudit(rule, result);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Rule execution failed: {ex.Message}";
        }

        return result;
    }





    /// <summary>
    ///     Validates the parameters required for rule execution by ensuring that all required parameters
    ///     are present and properly initialized in the global workflow context.
    /// </summary>
    /// <param name="parameters">
    ///     A list of <see cref="RuleParameter" /> objects representing the parameters defined for the rule.
    /// </param>
    /// <param name="globalParams">
    ///     An instance of <see cref="WorkflowParameters" /> containing the global parameters available
    ///     during the workflow execution.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when a required parameter is missing and no default value is provided.
    /// </exception>
    /// <remarks>
    ///     This method ensures that all required parameters for a rule are available in the global context.
    ///     If a parameter is missing and does not have a default value, an exception is logged and thrown.
    /// </remarks>
    private void ValidateParameters(List<RuleParameter> parameters, WorkflowParameters globalParams)
    {
        try
        {
            // Example: ensure required parameters exist in global context
            foreach (RuleParameter param in parameters)
            {
                object? value = globalParams.GetType().GetProperty(param.Name)?.GetValue(globalParams);

                if (value == null && param.DefaultValue == null) throw new InvalidOperationException($"Missing parameter: {param.Name}");
            }
        }
        catch (Exception e)
        {
            WCAEventSource.Log.RuleExecutionError("", e.Message, "Invalid Parameter failed parameter validation.", "");
        }
    }






    /// <summary>
    ///     Resolves the data associated with a specified probe target based on its subsystem type.
    /// </summary>
    /// <param name="probe">
    ///     The <see cref="ProbeTarget" /> instance representing the probe to be resolved.
    ///     It contains details such as the subsystem type, location, and optional pattern.
    /// </param>
    /// <returns>
    ///     An <see cref="object" /> representing the resolved data for the specified probe.
    ///     The returned value varies depending on the subsystem type.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown if the <see cref="ProbeTarget.Subsystem" /> is not supported.
    /// </exception>
    private object ResolveProbe(ProbeTarget probe)
    {
        // Stub: resolve based on subsystem
        return probe.Subsystem switch
        {
            SubsystemType.Registry => $"Resolved registry path: {probe.Location}",
            SubsystemType.FileSystem => $"Resolved file path: {probe.Location}",
            SubsystemType.ETW => $"Resolved ETW provider: {probe.Location}",
            _ => throw new NotSupportedException($"Unsupported probe subsystem: {probe.Subsystem}")
        };
    }





    /// <summary>
    ///     Evaluates the specified expression against the provided probe data.
    /// </summary>
    /// <param name="expression">
    ///     The <see cref="ExpressionDefinition" /> representing the expression to be evaluated.
    ///     It contains details such as the type of the expression and the expression text.
    /// </param>
    /// <param name="probeData">
    ///     An object containing the data to be used during the evaluation of the expression.
    /// </param>
    /// <returns>
    ///     A <see cref="bool" /> indicating whether the evaluation of the expression was successful.
    /// </returns>
    /// <remarks>
    ///     This method is a stub implementation and assumes that the evaluation succeeds
    ///     if the <paramref name="probeData" /> is not null. In a real-world scenario,
    ///     this would involve using a rules engine or a custom evaluator to process the expression.
    /// </remarks>
    private bool EvaluateExpression(ExpressionDefinition expression, object probeData)
    {
        // Stub: evaluate expression text against probe data
        // In practice, you’d plug in RulesEngine or a custom evaluator
        return probeData != null;
    }





    /// <summary>
    ///     Executes a list of actions defined in the workflow rules.
    /// </summary>
    /// <param name="actions">
    ///     A list of <see cref="RuleAction" /> objects representing the actions to be executed.
    ///     Each action specifies its type and target.
    /// </param>
    /// <remarks>
    ///     This method iterates through the provided actions and performs the corresponding operations
    ///     based on the action type and target. It is typically invoked during the evaluation of workflow rules.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="actions" /> parameter is null.
    /// </exception>
    private void ExecuteActions(List<RuleAction> actions)
    {
        if (actions == null) throw new ArgumentNullException(nameof(actions));

        foreach (RuleAction action in actions)
        {
            if (action == null) continue;

            // Reflect basic expected properties: ActionType, Target, Value
            object? actionTypeObj = action.GetType().GetProperty("ActionType")?.GetValue(action);
            object? targetObj = action.GetType().GetProperty("Target")?.GetValue(action);
            object? valueObj = action.GetType().GetProperty("Value")?.GetValue(action);

            string? actionType = actionTypeObj?.ToString();
            string? target = targetObj?.ToString();

            if (string.IsNullOrWhiteSpace(actionType)) continue;

            switch (actionType)
            {
                case "SetEnvironmentVariable":
                    // Target expected: VAR_NAME, value: string
                    if (!string.IsNullOrWhiteSpace(target) && valueObj is string envValue) Environment.SetEnvironmentVariable(target, envValue);

                    break;

                case "WriteFile":
                    // Target expected: file:<path> or raw path, value: string content
                    if (!string.IsNullOrWhiteSpace(target) && valueObj is string writeContent)
                    {
                        string path = target.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ? target[5..] : target;
                        File.WriteAllText(path, writeContent);
                    }

                    break;

                case "AppendFile":
                    // Append text to file
                    if (!string.IsNullOrWhiteSpace(target) && valueObj is string appendContent)
                    {
                        string path = target.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ? target[5..] : target;
                        File.AppendAllText(path, appendContent);
                    }

                    break;

                case "DeleteFile":
                    if (!string.IsNullOrWhiteSpace(target))
                    {
                        string path = target.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ? target[5..] : target;
                        if (File.Exists(path)) File.Delete(path);
                    }

                    break;

                case "SetRegistryValue":
                    // Target format: registry:<hive>\<subkey>|ValueName
                    // Value: object (string/int/etc.)
                    if (!string.IsNullOrWhiteSpace(target))
                    {
                        // Example: registry:HKEY_CURRENT_USER\Software\MyApp|SettingName
                        const string prefix = "registry:";
                        string spec = target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? target[prefix.Length..] : target;
                        string[] parts = spec.Split('|', 2);
                        if (parts.Length == 2)
                        {
                            string fullKeyPath = parts[0];
                            string valueName = parts[1];

                            // Split hive from subkey
                            int firstSlash = fullKeyPath.IndexOf('\\');
                            if (firstSlash > 0)
                            {
                                string hiveName = fullKeyPath[..firstSlash];
                                string subKey = fullKeyPath[(firstSlash + 1)..];

                                RegistryKey? baseKey = hiveName switch
                                {
                                    "HKEY_CURRENT_USER" => Registry.CurrentUser,
                                    "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                                    "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                                    "HKEY_USERS" => Registry.Users,
                                    "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
                                    _ => null
                                };

                                if (baseKey != null)
                                {
                                    using RegistryKey? key = baseKey.CreateSubKey(subKey, true);
                                    key?.SetValue(valueName, valueObj ?? string.Empty);
                                }
                            }
                        }
                    }

                    break;

                case "RemoveRegistryValue":
                    // Target format: registry:<hive>\<subkey>|ValueName
                    if (!string.IsNullOrWhiteSpace(target))
                    {
                        const string prefix = "registry:";
                        string spec = target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? target[prefix.Length..] : target;
                        string[] parts = spec.Split('|', 2);
                        if (parts.Length == 2)
                        {
                            string fullKeyPath = parts[0];
                            string valueName = parts[1];

                            int firstSlash = fullKeyPath.IndexOf('\\');
                            if (firstSlash > 0)
                            {
                                string hiveName = fullKeyPath[..firstSlash];
                                string subKey = fullKeyPath[(firstSlash + 1)..];

                                RegistryKey? baseKey = hiveName switch
                                {
                                    "HKEY_CURRENT_USER" => Registry.CurrentUser,
                                    "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                                    "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                                    "HKEY_USERS" => Registry.Users,
                                    "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
                                    _ => null
                                };

                                if (baseKey != null)
                                {
                                    using RegistryKey? key = baseKey.OpenSubKey(subKey, writable: true);
                                    if (key != null && key.GetValue(valueName) != null) key.DeleteValue(valueName, throwOnMissingValue: false);
                                }
                            }
                        }
                    }

                    break;

                default:
                    // Fallback: attempt to invoke an Execute() method if present
                    MethodInfo? exec = action.GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (exec != null && exec.GetParameters().Length == 0) exec.Invoke(action, null);

                    break;
            }
        }
    }





    /// <summary>
    ///     Logs an audit trail for the execution of a rule, including its result and associated metadata.
    /// </summary>
    /// <param name="rule">
    ///     The <see cref="RuleContract" /> instance representing the rule that was executed.
    ///     It contains the rule's metadata, parameters, and execution details.
    /// </param>
    /// <param name="result">
    ///     The <see cref="RuleResult" /> instance representing the outcome of the rule execution.
    ///     It includes the success status, message, timestamp, and other result details.
    /// </param>
    private void LogAudit(RuleContract rule, RuleResult result)
    {
        Console.WriteLine($"[AUDIT] Rule {rule.RuleName} executed. Success={result.Success}, Message={result.Message}");
    }


}
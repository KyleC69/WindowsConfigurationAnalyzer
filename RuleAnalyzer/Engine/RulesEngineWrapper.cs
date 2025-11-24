//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   RulesEngineWrapper.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Text.Json;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Helpers;

using ProbeFacts = KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Models.ProbeFacts;
using RuleResultArtifact = KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Models.RuleResultArtifact;

#endregion





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


public class RulesEngineWrapper
{


    //MS Rules Engine instance






    public RulesEngineWrapper(string rulesetJson)
    {
        // Accept either a single workflow JSON or an array of workflows
        Workflow[] workflows = JsonSerializer.Deserialize<Workflow[]>(rulesetJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                               ?? Array.Empty<Workflow>();

        if (workflows.Length == 0)
        {
            throw new ArgumentException("No workflows found in the provided JSON ruleset.");
        }

        RegisterHelpers();
    }





    // Register helpers into the engine so they can be used from expressions
    private void RegisterHelpers()
    {
        /*
          _engine.

              // expression functions use object[] signature; register strongly-typed wrappers
             RulesEngine.AddFunction("FileExists", new Func<string, bool>(RulesEngineHelpers.FileExists));
          _engine.AddFunction("ComputeFileSha256", new Func<string, string?>(RulesEngineHelpers.ComputeFileSha256));
          _engine.AddFunction("AreNamesEquivalent", new Func<IEnumerable<string>?, IEnumerable<string>?, bool>(RulesEngineHelpers.AreNamesEquivalent));
          _engine.AddFunction("EvaluateFileAcl", new Func<IEnumerable<AclEntry>?, object>(acl =>
          {
              var ok = RulesEngineHelpers.EvaluateFileAcl(acl, out Dictionary<string, object> evidence);

              return new { ok, evidence };
          }));
          _engine.AddFunction("EvaluateRegistryAcl", new Func<IEnumerable<AclEntry>?, object>(acl =>
          {
              var ok = RulesEngineHelpers.EvaluateRegistryAcl(acl, out Dictionary<string, object> evidence);

              return new { ok, evidence };
          }));
          _engine.AddFunction("VerifyManifestHash", new Func<string?, IEnumerable<RegistryKeySnapshot>?, object>((fileSha, snaps) =>
          {
              var ok = RulesEngineHelpers.VerifyManifestHash(fileSha, snaps, out Dictionary<string, object> evidence);

              return new { ok, evidence };
          }));
          _engine.AddFunction("NormalizeAclEntries", new Func<IEnumerable<string>?, List<AclEntry>>(RulesEngineHelpers.NormalizeAclEntries));
        */
    }





    /// <summary>
    ///     Executes the specified workflow using the provided facts and operator identity,
    ///     and returns a detailed rule result artifact containing the execution results.
    /// </summary>
    /// <param name="workflowName">
    ///     The name of the workflow to execute. This corresponds to a predefined workflow in the rules engine.
    /// </param>
    /// <param name="facts">
    ///     The <see cref="Models.ProbeFacts" /> object containing the input data and context required for rule evaluation.
    /// </param>
    /// <param name="operatorIdentity">
    ///     An optional identifier for the operator initiating the execution. Defaults to an empty string if not provided.
    /// </param>
    /// <returns>
    ///     A <see cref="Models.RuleResultArtifact" /> object containing the results of the rule execution,
    ///     including raw rule results, helper outputs, and additional metadata.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="workflowName" /> or <paramref name="facts" /> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the specified workflow cannot be found or executed.
    /// </exception>
    /// <remarks>
    ///     This method invokes all rules in the specified workflow and collects the results.
    ///     It also attempts to compute additional helper outputs for evidence purposes,
    ///     such as file hashes and name equivalence checks. Errors during helper output computation
    ///     are caught and do not affect the overall execution result.
    /// </remarks>
    public async Task<RuleResultArtifact> ExecuteAsync(string workflowName, ProbeFacts facts, string operatorIdentity = "")
    {
        object[] input = new object[] { facts };
        //List<RuleResultTree>? result = await _engine.ExecuteAllRulesAsync(workflowName, facts);
        RuleResultArtifact artifact = new()
        {
            WorkflowName = workflowName,
            Timestamp = DateTimeOffset.UtcNow,
            Facts = facts,
            OperatorIdentity = operatorIdentity ?? string.Empty
        };

        // Optionally extract helper outputs by invoking them directly for evidence
        try
        {
            Dictionary<string, object> helperOutputs = new()
            {
                ["manifestFileHash"] = RulesEngineHelpers.ComputeFileSha256(facts.Manifest?.Path ?? facts.ManifestPath ?? string.Empty) ?? string.Empty,
                ["areNamesEquivalent"] = RulesEngineHelpers.AreNamesEquivalent(facts.Manifest?.ChannelNames, facts.RegisteredProviders) // intentionally used as quick check
            };
            artifact = artifact with { HelperOutputs = helperOutputs };
        }
        catch
        {
            /* swallow helper evidence errors to preserve result artifact */
        }

        return artifact;
    }





    // Load JSON ruleset from file
    public static RulesEngineWrapper FromFile(string rulesJsonPath)
    {
        string json = File.ReadAllText(rulesJsonPath);

        return new RulesEngineWrapper(json);
    }


}
// Created:  2025/11/16
// Solution: WindowsConfigurationAnalyzer
// Project:  RuleAnalyzer
// File:  RulesEngineWrapper.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using System.Text.Json;

using KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Helpers;
using KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Models;

using RulesEngine.Models;





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


public class RulesEngineWrapper
{


    public  RulesEngine.RulesEngine _engine;





    public RulesEngineWrapper(string rulesetJson)
    {
        // Accept either a single workflow JSON or an array of workflows
        Workflow[] workflows = JsonSerializer.Deserialize<Workflow[]>(rulesetJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                               ?? Array.Empty<Workflow>();
        _engine = new RulesEngine.RulesEngine(workflows);
        RegisterHelpers();
    }





    // Register helpers into the engine so they can be used from expressions
    private void RegisterHelpers()
    {
      /*  
            // expression functions use object[] signature; register strongly-typed wrappers
            _engine.AddFunction("FileExists", new Func<string, bool>(RulesEngineHelpers.FileExists));
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





    // Execute a named workflow (or the only workflow) against facts
    public async Task<RuleResultArtifact> ExecuteAsync(string workflowName, ProbeFacts facts, string operatorIdentity = "")
    {
        var input = new object[] { facts };
        List<RuleResultTree>? result = await _engine.ExecuteAllRulesAsync(workflowName, facts);
        var artifact = new RuleResultArtifact
        {
            WorkflowName = workflowName,
            Timestamp = DateTimeOffset.UtcNow,
            Facts = facts,
            RawRuleResultTree = result,
            OperatorIdentity = operatorIdentity ?? string.Empty
        };

        // Optionally extract helper outputs by invoking them directly for evidence
        try
        {
            var helperOutputs = new Dictionary<string, object>
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
        var json = File.ReadAllText(rulesJsonPath);

        return new RulesEngineWrapper(json);
    }


}
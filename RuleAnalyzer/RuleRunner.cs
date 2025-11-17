using System.Reflection;
using System.Text.Json;



using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;
using KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Models;





// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer;


//Class will serve as the main entry point for running rules
public partial class RuleRunner
{
    public static string? ProjectDir => Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "ProjectDirectory")?.Value;



    public RuleRunner(IActivityLogger logger)
    {

    }

    public async Task RunRulesAsync()
    {
        var SolutionDir = Directory.GetParent(ProjectDir!)?.Parent?.Parent?.FullName;
        var RuleStore = Path.Combine(SolutionDir!, "RulesEngineStore");
        var rulesFiles= Directory.GetFiles(RuleStore, "*.json", SearchOption.AllDirectories);

        if (rulesFiles.Length == 0)
        {
            
            return;
        }

        string[] rulesJson = [];
        
   foreach (var i in rulesFiles)
        {
            var json = await File.ReadAllTextAsync(i);
            //rule = JsonSerializer.Deserialize<ProbeFacts>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ProbeFacts();
          //  jsonFacts = Directory.GetFiles(Path.GetDirectoryName(rulesFiles[i])!, "*.facts.json", SearchOption.TopDirectoryOnly);
        }

   


        //var engine = new RulesEngineWrapper(rulesJson);

        // workflow name must match the workflow inside ruleset.json
        var workflowName = "EventSourceProviderManifestRegistration";
        //var artifact = await engine.ExecuteAsync(workflowName, facts, operatorIdentity: Environment.UserName);

        // persist artifact for audit
        var outPath = $"rule-result-{DateTime.UtcNow:yyyyMMddTHHmmssZ}.json";
        //await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(artifact, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Rule execution completed. Artifact written to {outPath}");
    }

}








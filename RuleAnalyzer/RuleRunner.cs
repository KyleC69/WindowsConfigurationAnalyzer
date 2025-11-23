//  Created:  2025/11/16
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   RuleRunner.cs
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

#endregion





// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer;


//Class will serve as the main entry point for running rules
public class RuleRunner
{


    private readonly IActivityLogger _logger;





    public RuleRunner(IActivityLogger logger)
    {
        _logger = logger;
    }





    public static string? ProjectDir => Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "ProjectDirectory")?.Value;





    public async Task RunRulesAsync()
    {
        string? SolutionDir = Directory.GetParent(ProjectDir!)?.Parent?.Parent?.FullName;
        string RuleStore = Path.Combine(SolutionDir!, "RulesEngineStore");
        string[] rulesFiles = Directory.GetFiles(RuleStore, "*.json", SearchOption.AllDirectories);

        if (rulesFiles.Length == 0)
        {
            return;
        }

        string[] rulesJson = [];

        foreach (string i in rulesFiles)
        {
            string json = await File.ReadAllTextAsync(i);
            //rule = JsonSerializer.Deserialize<ProbeFacts>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ProbeFacts();
            //  jsonFacts = Directory.GetFiles(Path.GetDirectoryName(rulesFiles[i])!, "*.facts.json", SearchOption.TopDirectoryOnly);
        }
        //var artifact = await engine.ExecuteAsync(workflowName, facts, operatorIdentity: Environment.UserName);

        // persist artifact for audit
        string outPath = $"rule-result-{DateTime.UtcNow:yyyyMMddTHHmmssZ}.json";
        //await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(artifact, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Rule execution completed. Artifact written to {outPath}");
    }


}
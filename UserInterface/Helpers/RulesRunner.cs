//  Created:  2025/11/15
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   RulesRunner.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Text.Json;

using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Etw;

using Microsoft.Win32;

using RulesEngine.Models;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


public class RulesRunner
{


    public RulesRunner()
    {
        string? projDir = App.ProjectDir;
    }





    /// <summary>
    ///     Executes a set of rules asynchronously using the Rules Engine framework.
    /// </summary>
    /// <remarks>
    ///     This method reads a JSON file containing workflow definitions, deserializes it into a list of workflows,
    ///     and initializes a Rules Engine instance. It then executes all rules in the specified workflow against
    ///     a predefined input object representing user profile data. The results of the rule execution are evaluated
    ///     to determine whether all rules passed successfully, and the results are logged to the console.
    /// </remarks>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task RunRulesAsync()
    {
        //Testing method to run rules engine -- Feasibility study only
        string json = File.ReadAllText(Path.Combine(App.ProjectDir!, "Rules", "ProfileRule.json"));
        List<Workflow>? workflows = JsonSerializer.Deserialize<List<Workflow>>(json);
        List<RuleResultTree>? results;
        RulesEngine.RulesEngine rulesEngine = new(workflows?.ToArray());

        UserProfileInput input = GatherCurrentUserProfile();

        results = await rulesEngine.ExecuteAllRulesAsync("UserProfileBaseline", input);
        if (results == null || results.Count == 0)
        {
            ActivityLogger.Log("INF", "No results from rule execution.", "Rules Runner Failed");
            WCAEventSource.Log.RulesEngineError("No results from rule execution.");
            EventSource.SetCurrentThreadActivityId(Guid.Empty);
            WCAEventSource.Log.ActionStop("RunRulesAsync");

            return;
        }

        bool allPassed = results.All(r => r.IsSuccess);

        Console.WriteLine(allPassed
            ? "User profile baseline PASSED"
            : "User profile baseline FAILED");

        foreach (RuleResultTree result in results)
        {
            Console.WriteLine($"{result.Rule.RuleName}: {result.IsSuccess} - {result.Rule.SuccessEvent}");
        }
    }





    private string BuildRule()
    {
        StringBuilder sb = new();
        sb.AppendLine(
            "[\n  {\n    \"WorkflowName\": \"UserProfileBaseline\",\n    \"Rules\": [\n      { \"RuleName\": \"ProfilePathExists\", \"Expression\": \"Directory.Exists(input.ProfilePath)\" },\n      { \"RuleName\": \"NoTempProfile\", \"Expression\": \"!input.ProfilePath.Contains(\\\"TEMP\\\")\" },\n      { \"RuleName\": \"EnvVarMatch\", \"Expression\": \"input.UserProfileEnv == input.ProfilePath\" },\n      { \"RuleName\": \"StandardFoldersPresent\", \"Expression\": \"input.StandardFolders.All(f => Directory.Exists(f))\" },\n      { \"RuleName\": \"RegistryShellFoldersValid\", \"Expression\": \"input.RegistryShellFolders.All(p => Directory.Exists(p.Value))\" },\n      { \"RuleName\": \"AclFullControl\", \"Expression\": \"input.AclRights.Contains(\\\"FullControl\\\")\" },\n      { \"RuleName\": \"NoUnexpectedAces\", \"Expression\": \"!input.AclRights.Contains(\\\"Everyone:Write\\\")\" },\n      { \"RuleName\": \"ProfileServiceRunning\", \"Expression\": \"input.ProfSvcStatus == \\\"Running\\\"\" },\n      { \"RuleName\": \"NoTempProfileEvents\", \"Expression\": \"!input.EventIds.Contains(1511)\" },\n      { \"RuleName\": \"NoAccessDeniedEvents\", \"Expression\": \"!input.EventIds.Contains(1509)\" },\n      { \"RuleName\": \"RegistryProfileMatch\", \"Expression\": \"input.RegistryProfilePath == input.ProfilePath\" }\n    ]\n  }\n]");

        return sb.ToString();
    }





    private UserProfileInput GatherCurrentUserProfile()
    {
        UserProfileInput input = new()
        {
            UserProfileEnv = Environment.GetEnvironmentVariable("USERPROFILE") ?? string.Empty,
            ProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        // Registry profile path
        try
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            string? sid = identity.User?.Value;
            if (!string.IsNullOrWhiteSpace(sid))
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + sid);
                if (key != null) input.RegistryProfilePath = key.GetValue("ProfileImagePath") as string ?? string.Empty;
            }
        }
        catch
        {
            input.RegistryProfilePath = string.Empty;
        }

        // Standard folders
        input.StandardFolders = new List<string>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();

        // ACL rights
        try
        {
            if (Directory.Exists(input.ProfilePath))
            {
                DirectoryInfo dirInfo = new(input.ProfilePath);
                DirectorySecurity security = dirInfo.GetAccessControl();
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                HashSet<string> principalSids = [.. identity.Groups?.Select(g => g.Value) ?? Enumerable.Empty<string>()];
                if (identity.User?.Value is string userSid) principalSids.Add(userSid);

                foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(SecurityIdentifier)))
                {
                    string sid = rule.IdentityReference.Value;
                    if (rule.AccessControlType == AccessControlType.Allow && principalSids.Contains(sid))
                        foreach (FileSystemRights val in Enum.GetValues<FileSystemRights>())
                        {
                            if (val != 0 && rule.FileSystemRights.HasFlag(val))
                            {
                                string name = val.ToString();
                                if (!input.AclRights.Contains(name)) input.AclRights.Add(name);
                            }
                        }
                }
            }
        }
        catch
        {
            // Ignore ACL gathering errors
        }

        // Profile service status
        try
        {
            using ServiceController sc = new("ProfSvc");
            input.ProfSvcStatus = sc.Status.ToString();
        }
        catch
        {
            input.ProfSvcStatus = string.Empty;
        }

        // Event IDs (1509 / 1511 from User Profile Service) - last 1000 entries
        try
        {
            HashSet<int> ids = [];
            using EventLog log = new("Application");
            int count = log.Entries.Count;
            for (int i = count - 1; i >= 0 && i > count - 1000; i--)
            {
                EventLogEntry? entry = log.Entries[i];
                if (entry.Source == "User Profile Service" && (entry.InstanceId == 1509 || entry.InstanceId == 1511)) ids.Add((int)entry.InstanceId);
            }

            input.EventIds = ids.ToList();
        }
        catch
        {
            input.EventIds = [];
        }

        return input;
    }


}



public class UserProfileInput
{


    public string ProfilePath { get; set; } = string.Empty;
    public string UserProfileEnv { get; set; } = string.Empty;
    public string RegistryProfilePath { get; set; } = string.Empty;
    public List<string> StandardFolders { get; set; } = [];
    public List<string> AclRights { get; set; } = [];
    public string ProfSvcStatus { get; set; } = string.Empty;
    public List<int> EventIds { get; set; } = [];


}
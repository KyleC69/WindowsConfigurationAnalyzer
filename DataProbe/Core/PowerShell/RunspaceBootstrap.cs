//  Created:  2025/11/30
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   RunspaceBootstrap.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.PowerShell;


public static class RunspaceBootstrap
{


    public static Runspace CreateRunspace(IEnumerable<string>? extraModules = null)
    {
        var iss = InitialSessionState.CreateDefault2();
        var runspace = RunspaceFactory.CreateRunspace(iss);
        runspace.Open();

        runspace.SessionStateProxy.LanguageMode = PSLanguageMode.FullLanguage;

        // Ensure PSModulePath includes $PSHOME\Modules
        var psHome = Environment.GetEnvironmentVariable("PSHOME")
                     ?? @"C:\Program Files\PowerShell\7";
        var modulesPath = Path.Combine(psHome, "Modules");
        var existing = Environment.GetEnvironmentVariable("PSModulePath") ?? string.Empty;
        var newPath = $"{modulesPath};{existing}";
        runspace.SessionStateProxy.SetVariable("env:PSModulePath", newPath);

        // Import only if available
        var defaultModules = new[]
        {
            "Microsoft.PowerShell.Management",
            "Microsoft.PowerShell.Utility",
            "CimCmdlets"
        };

        ImportAvailableModules(runspace, defaultModules);
        if (extraModules != null) ImportAvailableModules(runspace, extraModules);

        Runspace.DefaultRunspace = runspace;

        return runspace;
    }





    private static void ImportAvailableModules(Runspace runspace, IEnumerable<string> modules)
    {
        // Prefer modules under PSHOME\Modules to avoid picking Windows PowerShell/legacy copies
        var psHome = Environment.GetEnvironmentVariable("PSHOME") ?? @"C:\Program Files\PowerShell\7";
        var modulesPath = Path.Combine(psHome, "Modules");

        using var ps = System.Management.Automation.PowerShell.Create();
        ps.Runspace = runspace;

        foreach (var module in modules)
        {
            ps.Commands.Clear();
            ps.AddCommand("Get-Module")
                .AddParameter("ListAvailable")
                .AddParameter("Name", module);

            Collection<PSObject>? found = ps.Invoke();
            if (found.Count > 0)
            {
                // Convert to PSModuleInfo candidates and choose preferred one (PSHOME\Modules first)
                List<PSModuleInfo?> candidates = found
                    .Select(f => f.BaseObject as PSModuleInfo)
                    .Where(m => m != null)
                    .ToList()!;

                PSModuleInfo? preferred = candidates
                                              .FirstOrDefault(m => !string.IsNullOrEmpty(m?.ModuleBase) &&
                                                                   m.ModuleBase.StartsWith(modulesPath, StringComparison.OrdinalIgnoreCase))
                                          ?? candidates.FirstOrDefault();

                if (preferred != null)
                {
                    // Import by path (manifest or module base) to avoid name-based ambiguity
                    var importPath = preferred.Path ?? preferred.ModuleBase ?? module;

                    ps.Commands.Clear();
                    ps.AddCommand("Import-Module").AddArgument(importPath);

                    try
                    {
                        ps.Invoke();
                        Console.WriteLine($"[RunspaceBootstrap] Imported {module} from {importPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[RunspaceBootstrap] Failed to import {module} ({importPath}): {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"[RunspaceBootstrap] No usable candidate for {module}, skipping.");
                }
            }
            else
            {
                Console.WriteLine($"[RunspaceBootstrap] Module {module} not found, skipping.");
            }
        }
    }


}
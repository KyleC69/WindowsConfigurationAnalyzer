//  Created:  2025/10/30
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   PolicyAnalyzer.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Xml;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Policy;


public sealed class PolicyAnalyzer : IAnalyzerModule
{


    private IActivityLogger? _logger;


    public string Name => "Policy/GPO Analyzer";
    public string Area => "Policy/GPO";





    public async Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger;
        string area = Area;
        _logger.Log("INF", "Start: Collecting policy and GPO data", area);
        List<string> warnings = [];
        List<string> errors = [];

        Dictionary<string, object?> policies = [];
        Dictionary<string, object?> defenderPolicies = [];
        Dictionary<string, object?> firewallPolicies = [];
        List<object> rsopComputerGpos = [];
        List<object> rsopUserGpos = [];
        List<object> rsopRegistry = [];
        List<object> registryPolFiles = [];

        try
        {
            _logger.Log("INF", "Tree: Start", area);
            foreach (string? root in new[] { "HKLM", "HKCU" })
            {
                cancellationToken.ThrowIfCancellationRequested();
                string basePath = $"{root}\\SOFTWARE\\Policies";
                EnumeratePolicyTree(context, basePath, policies, 12);
            }

            _logger.Log("INF", $"Tree: Complete: entries={policies.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Policy tree enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Tree: Policy tree enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "Targeted: Start", area);
            ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System",
                new List<string> { "EnableLUA", "ConsentPromptBehaviorAdmin", "PromptOnSecureDesktop", "EnableInstallerDetection", "DontDisplayLastUserName" }.AsReadOnly());
            ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\DomainProfile", new List<string> { "EnableFirewall", "DefaultInboundAction", "DefaultOutboundAction" }.AsReadOnly());
            ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\PrivateProfile", new List<string> { "EnableFirewall", "DefaultInboundAction", "DefaultOutboundAction" }.AsReadOnly());
            ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\PublicProfile", new List<string> { "EnableFirewall", "DefaultInboundAction", "DefaultOutboundAction" }.AsReadOnly());
            ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\DNSClient", new List<string> { "DisableSmartNameResolution", "EnableMulticast" }.AsReadOnly());
            ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\Terminal Services", new List<string> { "fDenyTSConnections", "UserAuthentication", "fSingleSessionPerUser" }.AsReadOnly());
            ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU", new List<string> { "NoAutoUpdate", "AUOptions", "ScheduledInstallDay", "ScheduledInstallTime" }.AsReadOnly());
            ReadPolicy(context, policies, "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa", new List<string> { "LimitBlankPasswordUse", "LmCompatibilityLevel", "RestrictAnonymous", "RestrictAnonymousSAM" }.AsReadOnly());
            _logger.Log("INF", "Targeted: Complete", area);
        }
        catch (Exception ex)
        {
            warnings.Add($"Targeted policy read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Targeted: Targeted policy read failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "Defender: Start", area);
            foreach (string? path in new[]
                     {
                         "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender", "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection",
                         "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet", "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Threats"
                     })
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnumeratePolicyTree(context, path, defenderPolicies, 6);
            }

            _logger.Log("INF", $"Defender: Complete: entries={defenderPolicies.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Defender policy enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Defender: Defender policy enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "Firewall: Start", area);
            foreach (string? path in new[]
                     {
                         "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\DomainProfile", "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\PrivateProfile",
                         "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\PublicProfile", "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\FirewallRules"
                     })
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnumeratePolicyTree(context, path, firewallPolicies, 6);
            }

            _logger.Log("INF", $"Firewall: Complete: entries={firewallPolicies.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Firewall policy enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Firewall: Firewall policy enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "RSOP: Start", area);
            try
            {
                IReadOnlyList<IDictionary<string, object?>> compGpos = await context.Cim.QueryAsync("SELECT Name, id, precedence FROM RSOP_GPO", "\\\\.\\root\\RSOP\\Computer", cancellationToken).ConfigureAwait(false);
                foreach (IDictionary<string, object?> gpo in compGpos)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    rsopComputerGpos.Add(new { Name = gpo.GetOrDefault("Name"), Id = gpo.GetOrDefault("id"), Precedence = gpo.GetOrDefault("precedence") });
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                warnings.Add($"RSOP Computer GPO query failed: {ex.Message}");
                errors.Add(ex.ToString());
                _logger.Log("ERR", $"RSOP: RSOP Computer GPO query failed ({ex.Message})", area);
            }

            try
            {
                IReadOnlyList<IDictionary<string, object?>> userGpos = await context.Cim.QueryAsync("SELECT Name, id, precedence FROM RSOP_GPO", "\\\\.\\root\\RSOP\\User", cancellationToken).ConfigureAwait(false);
                foreach (IDictionary<string, object?> gpo in userGpos)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    rsopUserGpos.Add(new { Name = gpo.GetOrDefault("Name"), Id = gpo.GetOrDefault("id"), Precedence = gpo.GetOrDefault("precedence") });
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                warnings.Add($"RSOP User GPO query failed: {ex.Message}");
                errors.Add(ex.ToString());
                _logger.Log("ERR", $"RSOP: RSOP User GPO query failed ({ex.Message})", area);
            }

            try
            {
                IReadOnlyList<IDictionary<string, object?>> regComp =
                    await context.Cim.QueryAsync("SELECT KeyName, ValueName, Value, GPOID FROM RSOP_RegistryPolicySetting", "\\\\.\\root\\RSOP\\Computer", cancellationToken).ConfigureAwait(false);
                foreach (IDictionary<string, object?> s in regComp)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    rsopRegistry.Add(new { Scope = "Computer", Key = s.GetOrDefault("KeyName"), Name = s.GetOrDefault("ValueName"), Value = s.GetOrDefault("Value"), Gpo = s.GetOrDefault("GPOID") });
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                warnings.Add($"RSOP Registry (Computer) query failed: {ex.Message}");
                errors.Add(ex.ToString());
                _logger.Log("ERR", $"RSOP: RSOP Registry (Computer) query failed ({ex.Message})", area);
            }

            try
            {
                IReadOnlyList<IDictionary<string, object?>> regUser = await context.Cim.QueryAsync("SELECT KeyName, ValueName, Value, GPOID FROM RSOP_RegistryPolicySetting", "\\\\.\\root\\RSOP\\User", cancellationToken)
                    .ConfigureAwait(false);
                foreach (IDictionary<string, object?> s in regUser)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    rsopRegistry.Add(new { Scope = "User", Key = s.GetOrDefault("KeyName"), Name = s.GetOrDefault("ValueName"), Value = s.GetOrDefault("Value"), Gpo = s.GetOrDefault("GPOID") });
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                warnings.Add($"RSOP Registry (User) query failed: {ex.Message}");
                errors.Add(ex.ToString());
                _logger.Log("ERR", $"RSOP: RSOP Registry (User) query failed ({ex.Message})", area);
            }

            _logger.Log("INF", $"RSOP: Complete: compGPOs={rsopComputerGpos.Count}, userGPOs={rsopUserGpos.Count}, registry={rsopRegistry.Count}", area);
        }
        catch
        {
        }

        try
        {
            _logger.Log("INF", "RegistryPol: Start", area);
            string sys = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string machinePol = Path.Combine(sys, "System32", "GroupPolicy", "Machine", "Registry.pol");
            string userPol = Path.Combine(sys, "System32", "GroupPolicy", "User", "Registry.pol");
            foreach ((string Scope, string Path) pol in new[] { (Scope: "Machine", Path: machinePol), (Scope: "User", Path: userPol) })
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (File.Exists(pol.Path))
                {
                    FileInfo fi = new(pol.Path);
                    registryPolFiles.Add(new { pol.Scope, pol.Path, Size = fi.Length, LastWriteUtc = fi.LastWriteTimeUtc });
                }
                else
                {
                    registryPolFiles.Add(new { pol.Scope, pol.Path, Size = 0L, LastWriteUtc = (DateTime?)null });
                }
            }

            _logger.Log("INF", "RegistryPol: Complete", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Registry.pol metadata enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"RegistryPol: Registry.pol metadata enumeration failed ({ex.Message})", area);
        }

        List<object> admxResults = [];
        List<object> compliance = [];
        try
        {
            _logger.Log("INF", "ADMX: Start", area);
            string policyDefFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "PolicyDefinitions");
            string locale = "en-US";
            if (Directory.Exists(policyDefFolder))
            {
                foreach (string admx in Directory.EnumerateFiles(policyDefFolder, "*.admx", SearchOption.TopDirectoryOnly))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        AdmxValidator.Result r = AdmxValidator.Validate(admx, Path.Combine(policyDefFolder, locale));
                        admxResults.Add(new { r.File, r.IsXmlValid, r.HasAdml, r.Root, r.State, r.Error });
                    }
                    catch (Exception vex)
                    {
                        warnings.Add($"ADMX validate failed: {Path.GetFileName(admx)}: {vex.Message}");
                    }
                }

                IEnumerable<(string key, string valueName)> definedPairs = BuildAdmxRegistryMap(policyDefFolder);
                HashSet<string> definedSet = new(definedPairs.Select(p => NormalizeKey(p.key) + ":" + p.valueName), StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, object?> kvp in policies)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string[] split = kvp.Key.Split(':');

                    if (split.Length != 2)
                    {
                        continue;
                    }

                    string hivePlusKey = split[0];
                    string valName = split[1];
                    string norm = NormalizeKey(hivePlusKey) + ":" + valName;
                    if (!definedSet.Contains(norm))
                    {
                        compliance.Add(new { Key = hivePlusKey, ValueName = valName, State = "UnknownInADMX" });
                    }
                }
            }
            else
            {
                warnings.Add("PolicyDefinitions folder not found");
            }

            _logger.Log("INF", "ADMX: Complete", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"ADMX verification failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"ADMX: ADMX verification failed ({ex.Message})", area);
        }

        var summary = new
        {
            PolicyEntries = policies.Count,
            DefenderEntries = defenderPolicies.Count,
            FirewallEntries = firewallPolicies.Count,
            RsopComputerGpos = rsopComputerGpos.Count,
            RsopUserGpos = rsopUserGpos.Count
        };
        var details = new
        {
            Policies = policies,
            Defender = defenderPolicies,
            Firewall = firewallPolicies,
            RSOP = new { ComputerGPOs = rsopComputerGpos, UserGPOs = rsopUserGpos, Registry = rsopRegistry },
            RegistryPol = registryPolFiles,
            Admx = admxResults,
            PolicyCompliance = compliance
        };
        AreaResult result = new(area, summary, details, new List<Finding>().AsReadOnly(), warnings, errors);
        _logger.Log("INF", "Complete: Policy and GPO collection completed", area);

        return result;
    }





    private static void ReadPolicy(IAnalyzerContext context, IDictionary<string, object?> bag, string baseKey, IEnumerable<string> names)
    {
        foreach (string name in names)
        {
            try
            {
                bag[$"{baseKey}:{name}"] = context.Registry.GetValue(baseKey, name);
            }
            catch
            {
            }
        }
    }





    private static void EnumeratePolicyTree(IAnalyzerContext context, string baseKey, IDictionary<string, object?> bag, int maxDepth, int depth = 0)
    {
        if (depth > maxDepth)
        {
            return;
        }

        try
        {
            foreach (string name in context.Registry.EnumerateValueNames(baseKey))
            {
                try
                {
                    bag[$"{baseKey}:{name}"] = context.Registry.GetValue(baseKey, name);
                }
                catch
                {
                }
            }

            foreach (string sub in context.Registry.EnumerateSubKeys(baseKey))
            {
                EnumeratePolicyTree(context, $"{baseKey}\\{sub}", bag, maxDepth, depth + 1);
            }
        }
        catch
        {
        }
    }





    private static IEnumerable<(string key, string valueName)> BuildAdmxRegistryMap(string policyDefFolder)
    {
        List<(string key, string valueName)> list = [];
        foreach (string admx in Directory.EnumerateFiles(policyDefFolder, "*.admx", SearchOption.TopDirectoryOnly))
        {
            try
            {
                using FileStream fs = File.OpenRead(admx);
                using XmlReader xr = XmlReader.Create(fs);
                while (xr.Read())
                {
                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        string? key = xr.GetAttribute("key");
                        string? valName = xr.GetAttribute("valueName") ?? xr.GetAttribute("name");
                        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(valName))
                        {
                            list.Add((key!, valName!));
                        }
                    }
                }
            }
            catch
            {
            }
        }

        return list;
    }





    private static string NormalizeKey(string hivePlusKey)
    {
        return hivePlusKey.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase) ? hivePlusKey.Substring(5)
            : hivePlusKey.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase) ? hivePlusKey.Substring(5) : hivePlusKey;
    }


}
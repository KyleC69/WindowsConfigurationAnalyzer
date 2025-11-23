//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   SecurityAnalyzer.cs
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
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Security;


public sealed class SecurityAnalyzer : IAnalyzerModule
{


    private IActivityLogger? _logger;


    public string Name => "Security Analyzer";
    public string Area => "Security";





    public async Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger;
        string area = Area;
        _logger.Log("INF", "Start: Collecting security configuration", area);
        List<string> warnings = [];
        List<string> errors = [];

        Dictionary<string, object?> secCenter = [];
        Dictionary<string, object?> defender = [];
        Dictionary<string, object?> deviceGuard = [];
        Dictionary<string, object?> secureBoot = [];
        List<object> bitlocker = [];
        Dictionary<string, object?> rdp = [];
        Dictionary<string, object?> smb = [];
        Dictionary<string, object?> lsa = [];
        Dictionary<string, object?> windowsUpdate = [];

        int avCount = 0;
        bool? uacEnabled = null;
        bool? secureBootEnabled = null;
        int bitlockerProtected = 0;

        // Windows Security Center (AV/AS/Firewall)
        try
        {
            _logger.Log("INF", "SecCenter: Start", area);
            List<object> av = [];
            List<object> fw = [];
            List<object> asw = [];
            try
            {
                IReadOnlyList<IDictionary<string, object?>> avRows = await context.Cim.QueryAsync(
                    "SELECT displayName, pathToSignedProductExe, productState, timestamp FROM AntiVirusProduct",
                    "\\\\.\\root\\SecurityCenter2", cancellationToken).ConfigureAwait(false);
                foreach (IDictionary<string, object?> mo in avRows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    av.Add(new Dictionary<string, object?>
                    {
                        ["Name"] = mo.GetOrDefault("displayName"),
                        ["Path"] = mo.GetOrDefault("pathToSignedProductExe"),
                        ["State"] = mo.GetOrDefault("productState"),
                        ["Timestamp"] = mo.GetOrDefault("timestamp")
                    });
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                warnings.Add($"SecurityCenter2 AntiVirusProduct query failed: {ex.Message}");
                errors.Add(ex.ToString());
                _logger.Log("ERR", $"SecCenter: AntiVirusProduct query failed ({ex.Message})", area);
            }

            try
            {
                IReadOnlyList<IDictionary<string, object?>> fwRows = await context.Cim.QueryAsync(
                    "SELECT displayName, pathToSignedProductExe, productState, timestamp FROM FirewallProduct",
                    "\\\\.\\root\\SecurityCenter2", cancellationToken).ConfigureAwait(false);
                foreach (IDictionary<string, object?> mo in fwRows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    fw.Add(new
                    {
                        Name = mo.GetOrDefault("displayName"),
                        Path = mo.GetOrDefault("pathToSignedProductExe"),
                        State = mo.GetOrDefault("productState"),
                        Timestamp = mo.GetOrDefault("timestamp")
                    });
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                warnings.Add($"SecurityCenter2 FirewallProduct query failed: {ex.Message}");
                errors.Add(ex.ToString());
                _logger.Log("ERR", $"SecCenter: FirewallProduct query failed ({ex.Message})", area);
            }

            try
            {
                IReadOnlyList<IDictionary<string, object?>> asRows = await context.Cim.QueryAsync(
                    "SELECT displayName, pathToSignedProductExe, productState, timestamp FROM AntiSpywareProduct",
                    "\\\\.\\root\\SecurityCenter2", cancellationToken).ConfigureAwait(false);
                foreach (IDictionary<string, object?> mo in asRows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    asw.Add(new
                    {
                        Name = mo.GetOrDefault("displayName"),
                        Path = mo.GetOrDefault("pathToSignedProductExe"),
                        State = mo.GetOrDefault("productState"),
                        Timestamp = mo.GetOrDefault("timestamp")
                    });
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                warnings.Add($"SecurityCenter2 AntiSpywareProduct query failed: {ex.Message}");
                errors.Add(ex.ToString());
                _logger.Log("ERR", $"SecCenter: AntiSpywareProduct query failed ({ex.Message})", area);
            }

            secCenter["Antivirus"] = av;
            secCenter["Firewall"] = fw;
            secCenter["AntiSpyware"] = asw;
            avCount = av.Count;
            _logger.Log("INF", $"SecCenter: Complete: AV={av.Count}, FW={fw.Count}, AS={asw.Count}", area);
        }
        catch
        {
        }

        // UAC basic
        try
        {
            _logger.Log("INF", "UAC: Start", area);
            object? v = context.Registry.GetValue(
                "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "EnableLUA");
            uacEnabled = v is int i && i != 0;
            lsa["UAC_EnableLUA"] = v;
            _logger.Log("INF", $"UAC: Complete: EnableLUA={v ?? "null"}", area);
        }
        catch (Exception ex)
        {
            warnings.Add($"UAC read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"UAC: UAC read failed ({ex.Message})", area);
        }

        // Defender service and signatures
        try
        {
            _logger.Log("INF", "Defender: Start", area);
            List<object> services = [];
            IReadOnlyList<IDictionary<string, object?>> svcRows = await context.Cim.QueryAsync(
                "SELECT Name, State, StartMode, PathName, DisplayName FROM Win32_Service WHERE Name='WinDefend' OR Name='Sense'",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> s in svcRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                services.Add(new
                {
                    Name = s.GetOrDefault("Name"),
                    State = s.GetOrDefault("State"),
                    StartMode = s.GetOrDefault("StartMode"),
                    PathName = s.GetOrDefault("PathName"),
                    DisplayName = s.GetOrDefault("DisplayName")
                });
            }

            defender["Services"] = services;
            Dictionary<string, object?> sig = [];
            foreach ((string? k, string[]? n) in new (string key, string[] names)[]
                     {
                         ("HKLM\\SOFTWARE\\Microsoft\\Windows Defender",
                             ["EngineVersion", "SignatureVersion", "AVSignatureVersion", "ASSignatureVersion"]),
                         ("HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Signature Updates",
                             ["EngineVersion", "AVSignatureVersion", "IASignatureVersion", "ASSignatureVersion", "LastUpdateTime"])
                     })
            {
                foreach (string name in n)
                {
                    try
                    {
                        sig[$"{k}:{name}"] = context.Registry.GetValue(k, name);
                    }
                    catch
                    {
                    }
                }
            }

            defender["Signatures"] = sig;
            _logger.Log("INF", "Defender: Complete", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Defender inspection failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Defender: Defender inspection failed ({ex.Message})", area);
        }

        // Device Guard / Credential Guard
        try
        {
            _logger.Log("INF", "DeviceGuard: Start", area);
            IReadOnlyList<IDictionary<string, object?>> dgRows = await context.Cim.QueryAsync(
                "SELECT * FROM Win32_DeviceGuard", "\\\\.\\root\\Microsoft\\Windows\\DeviceGuard", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> dg in dgRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                deviceGuard["SecurityServicesConfigured"] = dg.GetOrDefault("SecurityServicesConfigured");
                deviceGuard["SecurityServicesRunning"] = dg.GetOrDefault("SecurityServicesRunning");
                deviceGuard["VirtualizationBasedSecurityStatus"] = dg.GetOrDefault("VirtualizationBasedSecurityStatus");
                deviceGuard["HVCIProtectionLevel"] = dg.GetOrDefault("HVCIProtectionLevel");

                break;
            }

            try
            {
                lsa["RunAsPPL"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa", "RunAsPPL");
            }
            catch
            {
            }

            _logger.Log("INF", "DeviceGuard: Complete", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"DeviceGuard query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"DeviceGuard: DeviceGuard query failed ({ex.Message})", area);
        }

        // Secure Boot
        try
        {
            _logger.Log("INF", "SecureBoot: Start", area);
            IReadOnlyList<IDictionary<string, object?>> sbRows = await context.Cim.QueryAsync(
                "SELECT SecureBootEnabled FROM MS_SecureBoot", "\\\\.\\root\\wmi", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> sb in sbRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                object? v = sb.GetOrDefault("SecureBootEnabled");
                secureBootEnabled = v is uint ui ? ui != 0 : v is int ii && ii != 0;
                secureBoot["SecureBootEnabled"] = v;

                break;
            }

            _logger.Log("INF", $"SecureBoot: Complete: Enabled={secureBootEnabled?.ToString() ?? "null"} ", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Secure Boot query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"SecureBoot: Secure Boot query failed ({ex.Message})", area);
        }

        // BitLocker
        try
        {
            _logger.Log("INF", "BitLocker: Start", area);
            IReadOnlyList<IDictionary<string, object?>> blRows = await context.Cim.QueryAsync(
                "SELECT DeviceID, ProtectionStatus, EncryptionMethod FROM Win32_EncryptableVolume", "\\\\.\\root\\CIMV2\\Security\\MicrosoftVolumeEncryption", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> vol in blRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                object? ps = vol.GetOrDefault("ProtectionStatus");
                if (ps is uint and 1)
                {
                    bitlockerProtected++;
                }

                bitlocker.Add(new
                {
                    DeviceID = vol.GetOrDefault("DeviceID"),
                    ProtectionStatus = ps,
                    EncryptionMethod = vol.GetOrDefault("EncryptionMethod")
                });
            }

            _logger.Log("INF", $"BitLocker: Complete: protected={bitlockerProtected}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"BitLocker query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"BitLocker: BitLocker query failed ({ex.Message})", area);
        }

        // RDP/NLA
        try
        {
            _logger.Log("INF", "RDP: Start", area);
            rdp["fDenyTSConnections"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Terminal Server", "fDenyTSConnections");
            rdp["UserAuthentication"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp", "UserAuthentication");
            _logger.Log("INF", "RDP: Complete", area);
        }
        catch (Exception ex)
        {
            warnings.Add($"RDP read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"RDP: RDP read failed ({ex.Message})", area);
        }

        // SMBv1 state
        try
        {
            _logger.Log("INF", "SMB: Start", area);
            smb["Server_SMB1"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Services\\LanmanServer\\Parameters", "SMB1");
            smb["Client_MRxSmb10_Start"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Services\\mrxsmb10", "Start");
            smb["Client_MRxSmb20_Start"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Services\\mrxsmb20", "Start");
            _logger.Log("INF", "SMB: Complete", area);
        }
        catch (Exception ex)
        {
            warnings.Add($"SMB read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"SMB: SMB read failed ({ex.Message})", area);
        }

        // LSA hardening
        try
        {
            _logger.Log("INF", "LSA: Start", area);
            foreach (string? name in new[] { "LmCompatibilityLevel", "RestrictAnonymous", "RestrictAnonymousSAM", "NoLMHash" })
            {
                try
                {
                    lsa[name] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa", name);
                }
                catch
                {
                }
            }

            _logger.Log("INF", "LSA: Complete", area);
        }
        catch (Exception ex)
        {
            warnings.Add($"LSA read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"LSA: LSA read failed ({ex.Message})", area);
        }

        // Windows Update
        try
        {
            _logger.Log("INF", "WU: Start", area);
            IReadOnlyList<IDictionary<string, object?>> wuSvc = await context.Cim.QueryAsync(
                "SELECT Name, State, StartMode FROM Win32_Service WHERE Name='wuauserv'",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> s in wuSvc)
            {
                cancellationToken.ThrowIfCancellationRequested();
                windowsUpdate["Service_State"] = s.GetOrDefault("State");
                windowsUpdate["Service_StartMode"] = s.GetOrDefault("StartMode");
            }

            try
            {
                windowsUpdate["AUOptions"] = context.Registry.GetValue("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update", "AUOptions");
            }
            catch
            {
            }

            try
            {
                windowsUpdate["LastSuccessTime"] = context.Registry.GetValue("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\\Results\\Install", "LastSuccessTime");
            }
            catch
            {
            }

            _logger.Log("INF", "WU: Complete", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Windows Update inspection failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"WU: Windows Update inspection failed ({ex.Message})", area);
        }

        var summary = new
        {
            AntivirusProducts = avCount,
            UacEnabled = uacEnabled,
            SecureBootEnabled = secureBootEnabled,
            BitLockerProtectedVolumes = bitlockerProtected
        };
        var details = new
        {
            SecurityCenter = secCenter,
            Defender = defender,
            DeviceGuard = deviceGuard,
            SecureBoot = secureBoot,
            BitLocker = bitlocker,
            RDP = rdp,
            SMB = smb,
            LSA = lsa,
            WindowsUpdate = windowsUpdate
        };
        AreaResult result = new(area, summary, details, new List<Finding>().AsReadOnly(), warnings, errors);
        _logger.Log("INF", "Complete: Security configuration collected", area);

        return result;
    }


}
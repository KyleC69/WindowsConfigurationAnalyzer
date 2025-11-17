// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  SecurityAnalyzer.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Security;


public sealed class SecurityAnalyzer : IAnalyzerModule
{


    private IActivityLogger? _logger;


    public string Name => "Security Analyzer";
    public string Area => "Security";





    public async Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger;
        var area = Area;
        _logger.Log("INF", "Start: Collecting security configuration", area);
        List<string> warnings = new();
        List<string> errors = new();

        Dictionary<string, object?> secCenter = new();
        Dictionary<string, object?> defender = new();
        Dictionary<string, object?> deviceGuard = new();
        Dictionary<string, object?> secureBoot = new();
        List<object> bitlocker = new();
        Dictionary<string, object?> rdp = new();
        Dictionary<string, object?> smb = new();
        Dictionary<string, object?> lsa = new();
        Dictionary<string, object?> windowsUpdate = new();

        var avCount = 0;
        bool? uacEnabled = null;
        bool? secureBootEnabled = null;
        var bitlockerProtected = 0;

        // Windows Security Center (AV/AS/Firewall)
        try
        {
            _logger.Log("INF", "SecCenter: Start", area);
            List<object> av = new();
            List<object> fw = new();
            List<object> asw = new();
            try
            {
                var avRows = await context.Cim.QueryAsync(
                    "SELECT displayName, pathToSignedProductExe, productState, timestamp FROM AntiVirusProduct",
                    "\\\\.\\root\\SecurityCenter2", cancellationToken).ConfigureAwait(false);
                foreach (var mo in avRows)
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
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                warnings.Add($"SecurityCenter2 AntiVirusProduct query failed: {ex.Message}");
                errors.Add(ex.ToString());
                _logger.Log("ERR", $"SecCenter: AntiVirusProduct query failed ({ex.Message})", area);
            }

            try
            {
                var fwRows = await context.Cim.QueryAsync(
                    "SELECT displayName, pathToSignedProductExe, productState, timestamp FROM FirewallProduct",
                    "\\\\.\\root\\SecurityCenter2", cancellationToken).ConfigureAwait(false);
                foreach (var mo in fwRows)
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
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                warnings.Add($"SecurityCenter2 FirewallProduct query failed: {ex.Message}");
                errors.Add(ex.ToString());
                _logger.Log("ERR", $"SecCenter: FirewallProduct query failed ({ex.Message})", area);
            }

            try
            {
                var asRows = await context.Cim.QueryAsync(
                    "SELECT displayName, pathToSignedProductExe, productState, timestamp FROM AntiSpywareProduct",
                    "\\\\.\\root\\SecurityCenter2", cancellationToken).ConfigureAwait(false);
                foreach (var mo in asRows)
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
            catch (OperationCanceledException) { throw; }
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
        catch { }

        // UAC basic
        try
        {
            _logger.Log("INF", "UAC: Start", area);
            var v = context.Registry.GetValue(
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
            List<object> services = new();
            var svcRows = await context.Cim.QueryAsync(
                "SELECT Name, State, StartMode, PathName, DisplayName FROM Win32_Service WHERE Name='WinDefend' OR Name='Sense'",
                null, cancellationToken).ConfigureAwait(false);
            foreach (var s in svcRows)
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
            Dictionary<string, object?> sig = new();
            foreach (var (k, n) in new (string key, string[] names)[]
                     {
                         ("HKLM\\SOFTWARE\\Microsoft\\Windows Defender",
                             ["EngineVersion", "SignatureVersion", "AVSignatureVersion", "ASSignatureVersion"]),
                         ("HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Signature Updates",
                         ["EngineVersion", "AVSignatureVersion", "IASignatureVersion", "ASSignatureVersion", "LastUpdateTime"])
                     })
            {
                foreach (var name in n)
                {
                    try { sig[$"{k}:{name}"] = context.Registry.GetValue(k, name); } catch { }
                }
            }
            defender["Signatures"] = sig;
            _logger.Log("INF", "Defender: Complete", area);
        }
        catch (OperationCanceledException) { throw; }
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
            var dgRows = await context.Cim.QueryAsync(
                "SELECT * FROM Win32_DeviceGuard", "\\\\.\\root\\Microsoft\\Windows\\DeviceGuard", cancellationToken).ConfigureAwait(false);
            foreach (var dg in dgRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                deviceGuard["SecurityServicesConfigured"] = dg.GetOrDefault("SecurityServicesConfigured");
                deviceGuard["SecurityServicesRunning"] = dg.GetOrDefault("SecurityServicesRunning");
                deviceGuard["VirtualizationBasedSecurityStatus"] = dg.GetOrDefault("VirtualizationBasedSecurityStatus");
                deviceGuard["HVCIProtectionLevel"] = dg.GetOrDefault("HVCIProtectionLevel");
                break;
            }
            try { lsa["RunAsPPL"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa", "RunAsPPL"); } catch { }
            _logger.Log("INF", "DeviceGuard: Complete", area);
        }
        catch (OperationCanceledException) { throw; }
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
            var sbRows = await context.Cim.QueryAsync(
                "SELECT SecureBootEnabled FROM MS_SecureBoot", "\\\\.\\root\\wmi", cancellationToken).ConfigureAwait(false);
            foreach (var sb in sbRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var v = sb.GetOrDefault("SecureBootEnabled");
                secureBootEnabled = v is uint ui ? ui != 0 : v is int ii && ii != 0;
                secureBoot["SecureBootEnabled"] = v;
                break;
            }
            _logger.Log("INF", $"SecureBoot: Complete: Enabled={secureBootEnabled?.ToString() ?? "null"} ", area);
        }
        catch (OperationCanceledException) { throw; }
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
            var blRows = await context.Cim.QueryAsync(
                "SELECT DeviceID, ProtectionStatus, EncryptionMethod FROM Win32_EncryptableVolume", "\\\\.\\root\\CIMV2\\Security\\MicrosoftVolumeEncryption", cancellationToken).ConfigureAwait(false);
            foreach (var vol in blRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var ps = vol.GetOrDefault("ProtectionStatus");
                if (ps is uint u && u == 1)
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
        catch (OperationCanceledException) { throw; }
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
            foreach (var name in new[] { "LmCompatibilityLevel", "RestrictAnonymous", "RestrictAnonymousSAM", "NoLMHash" })
            {
                try { lsa[name] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa", name); } catch { }
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
            var wuSvc = await context.Cim.QueryAsync(
                "SELECT Name, State, StartMode FROM Win32_Service WHERE Name='wuauserv'",
                null, cancellationToken).ConfigureAwait(false);
            foreach (var s in wuSvc)
            {
                cancellationToken.ThrowIfCancellationRequested();
                windowsUpdate["Service_State"] = s.GetOrDefault("State");
                windowsUpdate["Service_StartMode"] = s.GetOrDefault("StartMode");
            }
            try { windowsUpdate["AUOptions"] = context.Registry.GetValue("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update", "AUOptions"); } catch { }
            try { windowsUpdate["LastSuccessTime"] = context.Registry.GetValue("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\\Results\\Install", "LastSuccessTime"); } catch { }
            _logger.Log("INF", "WU: Complete", area);
        }
        catch (OperationCanceledException) { throw; }
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
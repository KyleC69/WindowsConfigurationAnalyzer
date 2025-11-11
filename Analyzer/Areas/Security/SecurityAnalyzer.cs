// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  SecurityAnalyzer.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Security;



public sealed class SecurityAnalyzer : IAnalyzerModule
{
    public string Name => "Security Analyzer";
    public string Area => "Security";





    public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
    {
        string area = Area;
        context.ActionLogger.Info(area, "Start", "Collecting security configuration");
        List<string> warnings = new();
        List<string> errors = new();

        // Sections
        Dictionary<string, object?> secCenter = new();
        Dictionary<string, object?> defender = new();
        Dictionary<string, object?> deviceGuard = new();
        Dictionary<string, object?> secureBoot = new();
        List<object> bitlocker = new();
        Dictionary<string, object?> rdp = new();
        Dictionary<string, object?> smb = new();
        Dictionary<string, object?> lsa = new();
        Dictionary<string, object?> windowsUpdate = new();

        int avCount = 0;
        bool? uacEnabled = null;
        bool? secureBootEnabled = null;
        int bitlockerProtected = 0;

        // Windows Security Center (AV/AS/Firewall)
        try
        {
            context.ActionLogger.Info(area, "SecCenter", "Start");
            List<object> av = new();
            List<object> fw = new();
            List<object> asw = new();
            try
            {
                foreach (var mo in context.Cim.Query(
                             "SELECT displayName, pathToSignedProductExe, productState, timestamp FROM AntiVirusProduct",
                             "\\\\.\\root\\SecurityCenter2"))
                {
                    Dictionary<string, object?> item = new()
                    {
                        ["Name"] = mo.GetOrDefault("displayName"),
                        ["Path"] = mo.GetOrDefault("pathToSignedProductExe"),
                        ["State"] = mo.GetOrDefault("productState"),
                        ["Timestamp"] = mo.GetOrDefault("timestamp")
                    };
                    av.Add(item);
                }
            }
            catch (Exception ex)
            {
                warnings.Add($"SecurityCenter2 AntiVirusProduct query failed: {ex.Message}");
                errors.Add(ex.ToString());
                context.ActionLogger.Error(area, "SecCenter", "AntiVirusProduct query failed", ex);
            }

            try
            {
                foreach (var mo in context.Cim.Query(
                             "SELECT displayName, pathToSignedProductExe, productState, timestamp FROM FirewallProduct",
                             "\\\\.\\root\\SecurityCenter2"))
                {
                    fw.Add(new
                    {
                        Name = mo.GetOrDefault("displayName"),
                        Path = mo.GetOrDefault("pathToSignedProductExe"),
                        State = mo.GetOrDefault("productState"),
                        Timestamp = mo.GetOrDefault("timestamp")
                    });
                }
            }
            catch (Exception ex)
            {
                warnings.Add($"SecurityCenter2 FirewallProduct query failed: {ex.Message}");
                errors.Add(ex.ToString());
                context.ActionLogger.Error(area, "SecCenter", "FirewallProduct query failed", ex);
            }

            try
            {
                foreach (var mo in context.Cim.Query(
                             "SELECT displayName, pathToSignedProductExe, productState, timestamp FROM AntiSpywareProduct",
                             "\\\\.\\root\\SecurityCenter2"))
                {
                    asw.Add(new
                    {
                        Name = mo.GetOrDefault("displayName"),
                        Path = mo.GetOrDefault("pathToSignedProductExe"),
                        State = mo.GetOrDefault("productState"),
                        Timestamp = mo.GetOrDefault("timestamp")
                    });
                }
            }
            catch (Exception ex)
            {
                warnings.Add($"SecurityCenter2 AntiSpywareProduct query failed: {ex.Message}");
                errors.Add(ex.ToString());
                context.ActionLogger.Error(area, "SecCenter", "AntiSpywareProduct query failed", ex);
            }

            secCenter["Antivirus"] = av;
            secCenter["Firewall"] = fw;
            secCenter["AntiSpyware"] = asw;
            avCount = av.Count;
            context.ActionLogger.Info(area, "SecCenter", $"Complete: AV={av.Count}, FW={fw.Count}, AS={asw.Count}");
        }
        catch
        {
            /* guarded above */
        }

        // UAC basic
        try
        {
            context.ActionLogger.Info(area, "UAC", "Start");
            object? v = context.Registry.GetValue(
                "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System",
                "EnableLUA");
            uacEnabled = v is int i && i != 0;
            lsa["UAC_EnableLUA"] = v;
            context.ActionLogger.Info(area, "UAC", $"Complete: EnableLUA={v ?? "null"}");
        }
        catch (Exception ex)
        {
            warnings.Add($"UAC read failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "UAC", "UAC read failed", ex);
        }

        // Defender service and signatures
        try
        {
            context.ActionLogger.Info(area, "Defender", "Start");
            List<object> services = new();
            foreach (var s in context.Cim.Query(
                         "SELECT Name, State, StartMode, PathName, DisplayName FROM Win32_Service WHERE Name='WinDefend' OR Name='Sense'"))
            {
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
            // Signature info (best-effort; keys can vary by OS build)
            Dictionary<string, object?> sig = new();
            foreach ((string k, string[] n) in new (string key, string[] names)[]
                     {
                         ("HKLM\\SOFTWARE\\Microsoft\\Windows Defender",
                          new[] { "EngineVersion", "SignatureVersion", "AVSignatureVersion", "ASSignatureVersion" }),
                         ("HKLM\\SOFTWARE\\Microsoft\\Windows Defender\\Signature Updates",
                          new[]
                          {
                              "EngineVersion", "AVSignatureVersion", "IASignatureVersion", "ASSignatureVersion",
                              "LastUpdateTime"
                          })
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
            context.ActionLogger.Info(area, "Defender", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"Defender inspection failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Defender", "Defender inspection failed", ex);
        }

        // Device Guard / Credential Guard
        try
        {
            context.ActionLogger.Info(area, "DeviceGuard", "Start");
            foreach (var dg in context.Cim.Query("SELECT * FROM Win32_DeviceGuard",
                         "\\\\.\\root\\Microsoft\\Windows\\DeviceGuard"))
            {
                deviceGuard["SecurityServicesConfigured"] = dg.GetOrDefault("SecurityServicesConfigured");
                deviceGuard["SecurityServicesRunning"] = dg.GetOrDefault("SecurityServicesRunning");
                deviceGuard["VirtualizationBasedSecurityStatus"] = dg.GetOrDefault("VirtualizationBasedSecurityStatus");
                deviceGuard["HVCIProtectionLevel"] = dg.GetOrDefault("HVCIProtectionLevel");

                break;
            }

            // LSA Protection (RunAsPPL)
            try
            {
                lsa["RunAsPPL"] =
                    context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa", "RunAsPPL");
            }
            catch
            {
            }

            context.ActionLogger.Info(area, "DeviceGuard", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"DeviceGuard query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "DeviceGuard", "DeviceGuard query failed", ex);
        }

        // Secure Boot
        try
        {
            context.ActionLogger.Info(area, "SecureBoot", "Start");
            foreach (var sb in context.Cim.Query("SELECT SecureBootEnabled FROM MS_SecureBoot",
                         "\\\\.\\root\\wmi"))
            {
                object? v = sb.GetOrDefault("SecureBootEnabled");
                secureBootEnabled = v is uint ui ? ui != 0 : v is int ii && ii != 0;
                secureBoot["SecureBootEnabled"] = v;

                break;
            }

            context.ActionLogger.Info(area, "SecureBoot",
                $"Complete: Enabled={secureBootEnabled?.ToString() ?? "null"} ");
        }
        catch (Exception ex)
        {
            warnings.Add($"Secure Boot query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "SecureBoot", "Secure Boot query failed", ex);
        }

        // BitLocker
        try
        {
            context.ActionLogger.Info(area, "BitLocker", "Start");
            foreach (var vol in context.Cim.Query(
                         "SELECT DeviceID, ProtectionStatus, EncryptionMethod FROM Win32_EncryptableVolume",
                         "\\\\.\\root\\CIMV2\\Security\\MicrosoftVolumeEncryption"))
            {
                object? ps = vol.GetOrDefault("ProtectionStatus");
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

            context.ActionLogger.Info(area, "BitLocker", $"Complete: protected={bitlockerProtected}");
        }
        catch (Exception ex)
        {
            warnings.Add($"BitLocker query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "BitLocker", "BitLocker query failed", ex);
        }

        // RDP/NLA
        try
        {
            context.ActionLogger.Info(area, "RDP", "Start");
            rdp["fDenyTSConnections"] =
                context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Terminal Server",
                    "fDenyTSConnections");
            rdp["UserAuthentication"] = context.Registry.GetValue(
                "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Terminal Server\\WinStations\\RDP-Tcp",
                "UserAuthentication");
            context.ActionLogger.Info(area, "RDP", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"RDP read failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "RDP", "RDP read failed", ex);
        }

        // SMBv1 state
        try
        {
            context.ActionLogger.Info(area, "SMB", "Start");
            smb["Server_SMB1"] =
                context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Services\\LanmanServer\\Parameters",
                    "SMB1");
            smb["Client_MRxSmb10_Start"] =
                context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Services\\mrxsmb10", "Start");
            smb["Client_MRxSmb20_Start"] =
                context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Services\\mrxsmb20", "Start");
            context.ActionLogger.Info(area, "SMB", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"SMB read failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "SMB", "SMB read failed", ex);
        }

        // LSA hardening/security options
        try
        {
            context.ActionLogger.Info(area, "LSA", "Start");
            foreach (string name in new[]
                         { "LmCompatibilityLevel", "RestrictAnonymous", "RestrictAnonymousSAM", "NoLMHash" })
            {
                try
                {
                    lsa[name] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa", name);
                }
                catch
                {
                }
            }

            context.ActionLogger.Info(area, "LSA", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"LSA read failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "LSA", "LSA read failed", ex);
        }

        // Windows Update
        try
        {
            context.ActionLogger.Info(area, "WU", "Start");
            // Service state
            foreach (var s in context.Cim.Query(
                         "SELECT Name, State, StartMode FROM Win32_Service WHERE Name='wuauserv'"))
            {
                windowsUpdate["Service_State"] = s.GetOrDefault("State");
                windowsUpdate["Service_StartMode"] = s.GetOrDefault("StartMode");
            }

            // AU options and last success times (best-effort)
            try
            {
                windowsUpdate["AUOptions"] = context.Registry.GetValue(
                    "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update", "AUOptions");
            }
            catch
            {
            }

            try
            {
                windowsUpdate["LastSuccessTime"] = context.Registry.GetValue(
                    "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\\Results\\Install",
                    "LastSuccessTime");
            }
            catch
            {
            }

            context.ActionLogger.Info(area, "WU", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"Windows Update inspection failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "WU", "Windows Update inspection failed", ex);
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
        AreaResult result = new(area, summary, details, Array.Empty<Finding>(), warnings, errors);
        context.ActionLogger.Info(area, "Complete", "Security configuration collected");

        return Task.FromResult(result);
    }
}
// Created:  2025/10/30
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Xml;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Policy;



public sealed class PolicyAnalyzer : IAnalyzerModule
{
	public string Name => "Policy/GPO Analyzer";
	public string Area => "Policy/GPO";





	public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
	{
		var area = Area;
		context.ActionLogger.Info(area, "Start", "Collecting policy and GPO data");
		var warnings = new List<string>();
		var errors = new List<string>();

		// Accumulators
		var policies = new Dictionary<string, object?>();
		var defenderPolicies = new Dictionary<string, object?>();
		var firewallPolicies = new Dictionary<string, object?>();
		var rsopComputerGpos = new List<object>();
		var rsopUserGpos = new List<object>();
		var rsopRegistry = new List<object>();
		var registryPolFiles = new List<object>();

		//1) Recursive enumeration of Software\Policies trees
		try
		{
			context.ActionLogger.Info(area, "Tree", "Start");
			foreach (var root in new[] { "HKLM", "HKCU" })
			{
				var basePath = $"{root}\\SOFTWARE\\Policies";
				EnumeratePolicyTree(context, basePath, policies, 12);
			}

			context.ActionLogger.Info(area, "Tree", $"Complete: entries={policies.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Policy tree enumeration failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Tree", "Policy tree enumeration failed", ex);
		}

		//2) Targeted policy keys (UAC, Firewall, DNS Client, RDP, Windows Update, Security Options)
		try
		{
			context.ActionLogger.Info(area, "Targeted", "Start");
			ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", new[]
			{
				"EnableLUA", "ConsentPromptBehaviorAdmin", "PromptOnSecureDesktop", "EnableInstallerDetection",
				"DontDisplayLastUserName"
			});
			// Windows Firewall profiles
			ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\DomainProfile",
				new[] { "EnableFirewall", "DefaultInboundAction", "DefaultOutboundAction" });
			ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\PrivateProfile",
				new[] { "EnableFirewall", "DefaultInboundAction", "DefaultOutboundAction" });
			ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\PublicProfile",
				new[] { "EnableFirewall", "DefaultInboundAction", "DefaultOutboundAction" });
			// DNS client
			ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\DNSClient",
				new[] { "DisableSmartNameResolution", "EnableMulticast" });
			// RDP
			ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows NT\\Terminal Services",
				new[] { "fDenyTSConnections", "UserAuthentication", "fSingleSessionPerUser" });
			// Windows Update
			ReadPolicy(context, policies, "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU",
				new[] { "NoAutoUpdate", "AUOptions", "ScheduledInstallDay", "ScheduledInstallTime" });
			// Security options (LM/NTLM, anonymous restrictions)
			ReadPolicy(context, policies, "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa",
				new[] { "LimitBlankPasswordUse", "LmCompatibilityLevel", "RestrictAnonymous", "RestrictAnonymousSAM" });
			context.ActionLogger.Info(area, "Targeted", "Complete");
		}
		catch (Exception ex)
		{
			warnings.Add($"Targeted policy read failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Targeted", "Targeted policy read failed", ex);
		}

		//3) Defender policy trees
		try
		{
			context.ActionLogger.Info(area, "Defender", "Start");
			foreach (var path in new[]
			         {
				         "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender",
				         "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection",
				         "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Spynet",
				         "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Threats"
			         })
				EnumeratePolicyTree(context, path, defenderPolicies, 6);
			context.ActionLogger.Info(area, "Defender", $"Complete: entries={defenderPolicies.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Defender policy enumeration failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Defender", "Defender policy enumeration failed", ex);
		}

		//4) Firewall policy trees (complete)
		try
		{
			context.ActionLogger.Info(area, "Firewall", "Start");
			foreach (var path in new[]
			         {
				         "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\DomainProfile",
				         "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\PrivateProfile",
				         "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\PublicProfile",
				         "HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsFirewall\\FirewallRules"
			         })
				EnumeratePolicyTree(context, path, firewallPolicies, 6);
			context.ActionLogger.Info(area, "Firewall", $"Complete: entries={firewallPolicies.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Firewall policy enumeration failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Firewall", "Firewall policy enumeration failed", ex);
		}

		//5) RSOP - Computer and User GPO list and registry policy settings
		try
		{
			context.ActionLogger.Info(area, "RSOP", "Start");
			// GPO list - Computer
			try
			{
				foreach (var gpo in context.Cim.Query("SELECT Name, id, precedence FROM RSOP_GPO",
					         "\\\\.\\root\\RSOP\\Computer"))
					rsopComputerGpos.Add(new
					{
						Name = gpo.GetOrDefault("Name"), Id = gpo.GetOrDefault("id"),
						Precedence = gpo.GetOrDefault("precedence")
					});
			}
			catch (Exception ex)
			{
				warnings.Add($"RSOP Computer GPO query failed: {ex.Message}");
				errors.Add(ex.ToString());
				context.ActionLogger.Error(area, "RSOP", "RSOP Computer GPO query failed", ex);
			}

			// GPO list - User
			try
			{
				foreach (var gpo in context.Cim.Query("SELECT Name, id, precedence FROM RSOP_GPO",
					         "\\\\.\\root\\RSOP\\User"))
					rsopUserGpos.Add(new
					{
						Name = gpo.GetOrDefault("Name"), Id = gpo.GetOrDefault("id"),
						Precedence = gpo.GetOrDefault("precedence")
					});
			}
			catch (Exception ex)
			{
				warnings.Add($"RSOP User GPO query failed: {ex.Message}");
				errors.Add(ex.ToString());
				context.ActionLogger.Error(area, "RSOP", "RSOP User GPO query failed", ex);
			}

			// Registry policy settings (may be large; limit depth via selection)
			try
			{
				foreach (var s in context.Cim.Query(
					         "SELECT KeyName, ValueName, Value, GPOID FROM RSOP_RegistryPolicySetting",
					         "\\\\.\\root\\RSOP\\Computer"))
					rsopRegistry.Add(new
					{
						Scope = "Computer", Key = s.GetOrDefault("KeyName"), Name = s.GetOrDefault("ValueName"),
						Value = s.GetOrDefault("Value"), Gpo = s.GetOrDefault("GPOID")
					});
			}
			catch (Exception ex)
			{
				warnings.Add($"RSOP Registry (Computer) query failed: {ex.Message}");
				errors.Add(ex.ToString());
				context.ActionLogger.Error(area, "RSOP", "RSOP Registry (Computer) query failed", ex);
			}

			try
			{
				foreach (var s in context.Cim.Query(
					         "SELECT KeyName, ValueName, Value, GPOID FROM RSOP_RegistryPolicySetting",
					         "\\\\.\\root\\RSOP\\User"))
					rsopRegistry.Add(new
					{
						Scope = "User", Key = s.GetOrDefault("KeyName"), Name = s.GetOrDefault("ValueName"),
						Value = s.GetOrDefault("Value"), Gpo = s.GetOrDefault("GPOID")
					});
			}
			catch (Exception ex)
			{
				warnings.Add($"RSOP Registry (User) query failed: {ex.Message}");
				errors.Add(ex.ToString());
				context.ActionLogger.Error(area, "RSOP", "RSOP Registry (User) query failed", ex);
			}

			context.ActionLogger.Info(area, "RSOP",
				$"Complete: compGPOs={rsopComputerGpos.Count}, userGPOs={rsopUserGpos.Count}, registry={rsopRegistry.Count}");
		}
		catch
		{
			/* section-level guard */
		}

		//6) Registry.pol metadata
		try
		{
			context.ActionLogger.Info(area, "RegistryPol", "Start");
			var sys = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			var machinePol = Path.Combine(sys, "System32", "GroupPolicy", "Machine", "Registry.pol");
			var userPol = Path.Combine(sys, "System32", "GroupPolicy", "User", "Registry.pol");
			foreach (var pol in new[] { (Scope: "Machine", Path: machinePol), (Scope: "User", Path: userPol) })
				if (File.Exists(pol.Path))
				{
					var fi = new FileInfo(pol.Path);
					registryPolFiles.Add(new
						{ pol.Scope, pol.Path, Size = fi.Length, LastWriteUtc = fi.LastWriteTimeUtc });
				}
				else
				{
					registryPolFiles.Add(new { pol.Scope, pol.Path, Size = 0L, LastWriteUtc = (DateTime?)null });
				}

			context.ActionLogger.Info(area, "RegistryPol", "Complete");
		}
		catch (Exception ex)
		{
			warnings.Add($"Registry.pol metadata enumeration failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "RegistryPol", "Registry.pol metadata enumeration failed", ex);
		}

		// Existing ADMX checks and compliance mapping retained
		var admxResults = new List<object>();
		var compliance = new List<object>();
		try
		{
			context.ActionLogger.Info(area, "ADMX", "Start");
			var policyDefFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
				"PolicyDefinitions");
			var locale = "en-US"; // default; future: read system UI language
			if (Directory.Exists(policyDefFolder))
			{
				// Validate ADMXs
				foreach (var admx in Directory.EnumerateFiles(policyDefFolder, "*.admx", SearchOption.TopDirectoryOnly))
					try
					{
						var r = AdmxValidator.Validate(admx, Path.Combine(policyDefFolder, locale));
						admxResults.Add(new { r.File, r.IsXmlValid, r.HasAdml, r.Root, r.State, r.Error });
					}
					catch (Exception vex)
					{
						warnings.Add($"ADMX validate failed: {Path.GetFileName(admx)}: {vex.Message}");
					}

				// Build map of (key,valueName) from ADMX for rough compliance
				var definedPairs = BuildAdmxRegistryMap(policyDefFolder);
				var definedSet = new HashSet<string>(definedPairs.Select(p => NormalizeKey(p.key) + ":" + p.valueName),
					StringComparer.OrdinalIgnoreCase);
				foreach (var kvp in policies)
				{
					var split = kvp.Key.Split(':');

					if (split.Length != 2) continue;

					var hivePlusKey = split[0];
					var valName = split[1];
					var norm = NormalizeKey(hivePlusKey) + ":" + valName;
					var known = definedSet.Contains(norm);
					if (!known) compliance.Add(new { Key = hivePlusKey, ValueName = valName, State = "UnknownInADMX" });
				}
			}
			else
			{
				warnings.Add("PolicyDefinitions folder not found");
			}

			context.ActionLogger.Info(area, "ADMX", "Complete");
		}
		catch (Exception ex)
		{
			warnings.Add($"ADMX verification failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "ADMX", "ADMX verification failed", ex);
		}

		var summary = new
		{
			PolicyEntries = policies.Count, DefenderEntries = defenderPolicies.Count,
			FirewallEntries = firewallPolicies.Count, RsopComputerGpos = rsopComputerGpos.Count,
			RsopUserGpos = rsopUserGpos.Count
		};
		var details = new
		{
			Policies = policies, Defender = defenderPolicies, Firewall = firewallPolicies,
			RSOP = new { ComputerGPOs = rsopComputerGpos, UserGPOs = rsopUserGpos, Registry = rsopRegistry },
			RegistryPol = registryPolFiles, Admx = admxResults, PolicyCompliance = compliance
		};
		var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), warnings, errors);
		context.ActionLogger.Info(area, "Complete", "Policy and GPO collection completed");

		return Task.FromResult(result);
	}





	private static void ReadPolicy(IAnalyzerContext context, IDictionary<string, object?> bag, string baseKey,
		IEnumerable<string> names)
	{
		foreach (var name in names)
			try
			{
				var v = context.Registry.GetValue(baseKey, name);
				bag[$"{baseKey}:{name}"] = v;
			}
			catch
			{
				// continue on error
			}
	}





	private static void EnumeratePolicyTree(IAnalyzerContext context, string baseKey, IDictionary<string, object?> bag,
		int maxDepth, int depth = 0)
	{
		if (depth > maxDepth) return;

		try
		{
			foreach (var name in context.Registry.EnumerateValueNames(baseKey))
				try
				{
					bag[$"{baseKey}:{name}"] = context.Registry.GetValue(baseKey, name);
				}
				catch
				{
				}

			foreach (var sub in context.Registry.EnumerateSubKeys(baseKey))
				EnumeratePolicyTree(context, $"{baseKey}\\{sub}", bag, maxDepth, depth + 1);
		}
		catch
		{
		}
	}





	private static IEnumerable<(string key, string valueName)> BuildAdmxRegistryMap(string policyDefFolder)
	{
		var list = new List<(string key, string valueName)>();
		foreach (var admx in Directory.EnumerateFiles(policyDefFolder, "*.admx", SearchOption.TopDirectoryOnly))
			try
			{
				using var fs = File.OpenRead(admx);
				using var xr = XmlReader.Create(fs);
				while (xr.Read())
					if (xr.NodeType == XmlNodeType.Element)
					{
						var key = xr.GetAttribute("key");
						var valName = xr.GetAttribute("valueName") ?? xr.GetAttribute("name");
						if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(valName))
							list.Add((key!, valName!));
					}
			}
			catch
			{
			}

		return list;
	}





	private static string NormalizeKey(string hivePlusKey)
	{
		// Drop HKLM/HKCU prefix when present, for comparison with ADMX relative keys
		return hivePlusKey.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase)
			? hivePlusKey.Substring(5)
			: hivePlusKey.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase)
				? hivePlusKey.Substring(5)
				: hivePlusKey;
	}
}
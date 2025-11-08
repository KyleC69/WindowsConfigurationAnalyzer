// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Diagnostics;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Software;



public sealed class SoftwareAnalyzer : IAnalyzerModule
{
	public string Name => "Software Analyzer";
	public string Area => "Software";





	public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
	{
		var area = Area;
		context.ActionLogger.Info(area, "Start", "Collecting software inventory");
		var warnings = new List<string>();
		var errors = new List<string>();

		// Installed software (registry-based)
		var installed = new List<object>();
		try
		{
			context.ActionLogger.Info(area, "Installed", "Start");
			foreach (var relPath in new[]
			         {
				         "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall",
				         "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall"
			         })
			{
				ReadUninstallKey(context, installed, $"HKLM\\{relPath}");
				ReadUninstallKey(context, installed, $"HKCU\\{relPath}");
			}

			context.ActionLogger.Info(area, "Installed", $"Complete: count={installed.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Registry enumeration failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Installed", "Registry enumeration failed", ex);
		}

		// Running processes snapshot
		var processes = new List<object>();
		try
		{
			context.ActionLogger.Info(area, "Processes", "Start");
			foreach (var p in Process.GetProcesses())
			{
				var name = string.Empty;
				string? path = null;
				try
				{
					name = p.ProcessName;
					try
					{
						path = p.MainModule?.FileName;
					}
					catch
					{
						/* access denied */
					}
				}
				catch
				{
				}

				processes.Add(new { Name = name, Path = path, p.Id });
			}

			context.ActionLogger.Info(area, "Processes", $"Complete: count={processes.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Process enumeration failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Processes", "Process enumeration failed", ex);
		}

		// Optional Features (client/server)
		var optionalFeatures = new List<object>();
		try
		{
			context.ActionLogger.Info(area, "OptionalFeatures", "Start");
			foreach (var f in context.Cim.Query("SELECT Name, InstallState, Caption FROM Win32_OptionalFeature"))
				optionalFeatures.Add(new
				{
					Name = f.GetOrDefault("Name"), State = f.GetOrDefault("InstallState"),
					Caption = f.GetOrDefault("Caption")
				});
			context.ActionLogger.Info(area, "OptionalFeatures", $"Complete: count={optionalFeatures.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Optional features query failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "OptionalFeatures", "Optional features query failed", ex);
		}

		// Server Features (when present)
		var serverFeatures = new List<object>();
		try
		{
			context.ActionLogger.Info(area, "ServerFeatures", "Start");
			foreach (var sf in context.Cim.Query("SELECT ID, Name, InstallState FROM Win32_ServerFeature"))
				serverFeatures.Add(new
				{
					Id = sf.GetOrDefault("ID"), Name = sf.GetOrDefault("Name"), State = sf.GetOrDefault("InstallState")
				});
			context.ActionLogger.Info(area, "ServerFeatures", $"Complete: count={serverFeatures.Count}");
		}
		catch (Exception ex)
		{
			// This class may not exist on client SKUs
			warnings.Add($"Server features query failed or not supported: {ex.Message}");
			context.ActionLogger.Warn(area, "ServerFeatures",
				$"Server features query failed or not supported: {ex.Message}");
		}

		// Installed Store apps (best-effort WMI class)
		var storeApps = new List<object>();
		try
		{
			context.ActionLogger.Info(area, "StoreApps", "Start");
			foreach (var a in context.Cim.Query(
				         "SELECT Name, Version, InstallLocation, Publisher FROM Win32_InstalledStoreProgram"))
				storeApps.Add(new
				{
					Name = a.GetOrDefault("Name"), Version = a.GetOrDefault("Version"),
					InstallLocation = a.GetOrDefault("InstallLocation"), Publisher = a.GetOrDefault("Publisher")
				});
			context.ActionLogger.Info(area, "StoreApps", $"Complete: count={storeApps.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Store apps query failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "StoreApps", "Store apps query failed", ex);
		}

		// Provisioned Appx (registry best-effort)
		var provisionedAppx = new List<object>();
		try
		{
			context.ActionLogger.Info(area, "ProvisionedAppx", "Start");
			var root = "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore\\Applications";
			foreach (var app in context.Registry.EnumerateSubKeys(root))
			{
				var baseKey = $"{root}\\{app}";
				var fullName = context.Registry.GetValue(baseKey, "PackageFullName")?.ToString();
				var moniker = context.Registry.GetValue(baseKey, "PackageMoniker")?.ToString();
				var idName = context.Registry.GetValue(baseKey, "PackageIdName")?.ToString();
				provisionedAppx.Add(new
					{ Key = app, PackageFullName = fullName, PackageMoniker = moniker, PackageIdName = idName });
			}

			context.ActionLogger.Info(area, "ProvisionedAppx", $"Complete: count={provisionedAppx.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Provisioned Appx read failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "ProvisionedAppx", "Provisioned Appx read failed", ex);
		}

		// Provisioning packages on disk (best-effort)
		var provisioningPkgs = new List<object>();
		try
		{
			context.ActionLogger.Info(area, "ProvisioningPkgs", "Start");
			foreach (var dir in new[]
			         {
				         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Recovery",
					         "Customizations"),
				         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					         "Microsoft", "Provisioning", "Packages")
			         })
				if (Directory.Exists(dir))
					foreach (var ppkg in Directory.EnumerateFiles(dir, "*.ppkg", SearchOption.AllDirectories))
					{
						var fi = new FileInfo(ppkg);
						provisioningPkgs.Add(new { Path = ppkg, Size = fi.Length, LastWriteUtc = fi.LastWriteTimeUtc });
					}

			context.ActionLogger.Info(area, "ProvisioningPkgs", $"Complete: count={provisioningPkgs.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Provisioning packages scan failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "ProvisioningPkgs", "Provisioning packages scan failed", ex);
		}

		// Summary and return
		var summary = new
		{
			InstalledCount = installed.Count, RunningProcesses = processes.Count,
			OptionalFeatures = optionalFeatures.Count, StoreApps = storeApps.Count
		};
		var details = new
		{
			Installed = installed, RunningProcesses = processes, OptionalFeatures = optionalFeatures,
			ServerFeatures = serverFeatures, StoreApps = storeApps, ProvisionedAppx = provisionedAppx,
			ProvisioningPackages = provisioningPkgs
		};
		var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), warnings, errors);
		context.ActionLogger.Info(area, "Complete", "Software inventory collected");

		return Task.FromResult(result);
	}





	private static void ReadUninstallKey(IAnalyzerContext context, List<object> target, string hiveAndPath)
	{
		foreach (var sub in context.Registry.EnumerateSubKeys(hiveAndPath))
		{
			var basePath = $"{hiveAndPath}\\{sub}";
			var name = context.Registry.GetValue(basePath, "DisplayName")?.ToString();

			if (string.IsNullOrWhiteSpace(name)) continue; // skip non-display entries

			var ver = context.Registry.GetValue(basePath, "DisplayVersion")?.ToString();
			var pub = context.Registry.GetValue(basePath, "Publisher")?.ToString();
			var installDate = context.Registry.GetValue(basePath, "InstallDate")?.ToString();
			target.Add(new { Name = name, Version = ver, Publisher = pub, InstallDate = installDate, Key = basePath });
		}
	}
}
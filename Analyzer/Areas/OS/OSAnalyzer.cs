// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.OS;



public sealed class OSAnalyzer : IAnalyzerModule
{
	public string Name => "OS Analyzer";
	public string Area => "OS";





	public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
	{
		var area = Area;
		context.ActionLogger.Info(area, "Start", "Collecting OS and system information");
		var warnings = new List<string>();
		var errors = new List<string>();

		// Aggregates
		var system = new Dictionary<string, object?>();
		var os = new Dictionary<string, object?>();
		var bios = new Dictionary<string, object?>();
		var install = new Dictionary<string, object?>();
		var pendingReboot = new Dictionary<string, object?>();
		var services = new Dictionary<string, object?>();
		var servicesAutoIssues = new List<object>();
		var updates = new List<object>();
		var timeInfo = new Dictionary<string, object?>();
		var pagefile = new List<object>();
		var power = new Dictionary<string, object?>();
		var locale = new Dictionary<string, object?>();

		// Win32_ComputerSystem
		try
		{
			context.ActionLogger.Info(area, "ComputerSystem", "Start");
			foreach (var cs in context.Cim.Query(
				         "SELECT Manufacturer, Model, Domain, PartOfDomain, TotalPhysicalMemory, NumberOfProcessors, NumberOfLogicalProcessors, SystemType FROM Win32_ComputerSystem"))
			{
				system["Manufacturer"] = cs.GetOrDefault("Manufacturer");
				system["Model"] = cs.GetOrDefault("Model");
				system["Domain"] = cs.GetOrDefault("Domain");
				system["PartOfDomain"] = cs.GetOrDefault("PartOfDomain");
				system["TotalPhysicalMemory"] = cs.GetOrDefault("TotalPhysicalMemory");
				system["NumberOfProcessors"] = cs.GetOrDefault("NumberOfProcessors");
				system["NumberOfLogicalProcessors"] = cs.GetOrDefault("NumberOfLogicalProcessors");
				system["SystemType"] = cs.GetOrDefault("SystemType");

				break;
			}

			context.ActionLogger.Info(area, "ComputerSystem", "Complete");
		}
		catch (Exception ex)
		{
			warnings.Add($"ComputerSystem query failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "ComputerSystem", "ComputerSystem query failed", ex);
		}

		// Win32_OperatingSystem
		try
		{
			context.ActionLogger.Info(area, "OperatingSystem", "Start");
			foreach (var o in context.Cim.Query(
				         "SELECT Caption, Version, BuildNumber, CSDVersion, OSArchitecture, InstallDate, LastBootUpTime, SystemDirectory, WindowsDirectory, Locale, OSLanguage FROM Win32_OperatingSystem"))
			{
				os["Caption"] = o.GetOrDefault("Caption");
				os["Version"] = o.GetOrDefault("Version");
				os["BuildNumber"] = o.GetOrDefault("BuildNumber");
				os["CSDVersion"] = o.GetOrDefault("CSDVersion");
				os["OSArchitecture"] = o.GetOrDefault("OSArchitecture");
				os["SystemDirectory"] = o.GetOrDefault("SystemDirectory");
				os["WindowsDirectory"] = o.GetOrDefault("WindowsDirectory");
				os["Locale"] = o.GetOrDefault("Locale");
				os["OSLanguage"] = o.GetOrDefault("OSLanguage");
				os["InstallDateRaw"] = o.GetOrDefault("InstallDate");
				os["LastBootUpTimeRaw"] = o.GetOrDefault("LastBootUpTime");
				// Attempt to parse DMTF datetimes
				if (o.GetOrDefault("InstallDate") is string id && TryParseDmtfDate(id, out var instUtc))
					install["InstallDateUtc"] = instUtc;
				if (o.GetOrDefault("LastBootUpTime") is string lb && TryParseDmtfDate(lb, out var bootUtc))
					os["LastBootUpTimeUtc"] = bootUtc;

				break;
			}

			context.ActionLogger.Info(area, "OperatingSystem", "Complete");
		}
		catch (Exception ex)
		{
			warnings.Add($"OperatingSystem query failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "OperatingSystem", "OperatingSystem query failed", ex);
		}

		// Win32_BIOS and BaseBoard
		try
		{
			context.ActionLogger.Info(area, "BIOS", "Start");
			foreach (var b in context.Cim.Query(
				         "SELECT Manufacturer, SMBIOSBIOSVersion, SerialNumber, ReleaseDate FROM Win32_BIOS"))
			{
				bios["Manufacturer"] = b.GetOrDefault("Manufacturer");
				bios["SMBIOSBIOSVersion"] = b.GetOrDefault("SMBIOSBIOSVersion");
				bios["SerialNumber"] = b.GetOrDefault("SerialNumber");
				bios["ReleaseDateRaw"] = b.GetOrDefault("ReleaseDate");

				break;
			}

			context.ActionLogger.Info(area, "BIOS", "Complete");
		}
		catch (Exception ex)
		{
			warnings.Add($"BIOS query failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "BIOS", "BIOS query failed", ex);
		}

		// Pending reboot indicators (best-effort)
		try
		{
			context.ActionLogger.Info(area, "PendingReboot", "Start");
			pendingReboot["CBS_RebootPending"] = KeyExists(context,
				"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Component Based Servicing", "RebootPending");
			pendingReboot["WU_RebootRequired"] = KeyExists(context,
				"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update", "RebootRequired");
			try
			{
				pendingReboot["PendingFileRenameOperations"] =
					context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager",
						"PendingFileRenameOperations") is not null;
			}
			catch
			{
			}

			context.ActionLogger.Info(area, "PendingReboot", "Complete");
		}
		catch (Exception ex)
		{
			warnings.Add($"Pending reboot check failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "PendingReboot", "Pending reboot check failed", ex);
		}

		// Services summary and auto-start issues
		try
		{
			context.ActionLogger.Info(area, "Services", "Start");
			int running = 0, stopped = 0, paused = 0;
			foreach (var s in context.Cim.Query(
				         "SELECT Name, DisplayName, StartMode, State, PathName FROM Win32_Service"))
			{
				var state = s.GetOrDefault("State")?.ToString();
				var start = s.GetOrDefault("StartMode")?.ToString();
				if (string.Equals(state, "Running", StringComparison.OrdinalIgnoreCase))
					running++;
				else if (string.Equals(state, "Stopped", StringComparison.OrdinalIgnoreCase))
					stopped++;
				else if (string.Equals(state, "Paused", StringComparison.OrdinalIgnoreCase)) paused++;
				// Auto-start but not running (not including Delayed Auto handled in Startup analyzer)
				if (string.Equals(start, "Auto", StringComparison.OrdinalIgnoreCase) &&
				    !string.Equals(state, "Running", StringComparison.OrdinalIgnoreCase))
					servicesAutoIssues.Add(new
					{
						Name = s.GetOrDefault("Name"), DisplayName = s.GetOrDefault("DisplayName"), StartMode = start,
						State = state, Path = s.GetOrDefault("PathName")
					});
			}

			services["Running"] = running;
			services["Stopped"] = stopped;
			services["Paused"] = paused;
			services["AutoStartNotRunning"] = servicesAutoIssues;
			context.ActionLogger.Info(area, "Services",
				$"Complete: running={running}, stopped={stopped}, paused={paused}, autoIssues={servicesAutoIssues.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Service enumeration failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Services", "Service enumeration failed", ex);
		}

		// Installed Updates (QFE)
		try
		{
			context.ActionLogger.Info(area, "Updates", "Start");
			foreach (var q in context.Cim.Query(
				         "SELECT HotFixID, Description, InstalledOn FROM Win32_QuickFixEngineering"))
				updates.Add(new
				{
					HotFixID = q.GetOrDefault("HotFixID"), Description = q.GetOrDefault("Description"),
					InstalledOn = q.GetOrDefault("InstalledOn")
				});
			context.ActionLogger.Info(area, "Updates", $"Complete: count={updates.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Updates (QFE) query failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Updates", "Updates (QFE) query failed", ex);
		}

		// Timezone and W32Time service
		try
		{
			context.ActionLogger.Info(area, "Time", "Start");
			foreach (var tz in context.Cim.Query(
				         "SELECT Bias, Caption, DaylightBias, DaylightName, StandardName FROM Win32_TimeZone"))
			{
				timeInfo["TimeZone"] = new
				{
					tzCaption = tz.GetOrDefault("Caption"), StandardName = tz.GetOrDefault("StandardName"),
					DaylightName = tz.GetOrDefault("DaylightName")
				};

				break;
			}

			foreach (var s in context.Cim.Query(
				         "SELECT Name, State, StartMode FROM Win32_Service WHERE Name='W32Time'"))
			{
				timeInfo["W32Time_State"] = s.GetOrDefault("State");
				timeInfo["W32Time_StartMode"] = s.GetOrDefault("StartMode");
			}

			try
			{
				timeInfo["NtpServer"] =
					context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Services\\W32Time\\Parameters",
						"NtpServer");
			}
			catch
			{
			}

			try
			{
				timeInfo["Type"] =
					context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Services\\W32Time\\Parameters", "Type");
			}
			catch
			{
			}

			context.ActionLogger.Info(area, "Time", "Complete");
		}
		catch (Exception ex)
		{
			warnings.Add($"Time/W32Time query failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Time", "Time/W32Time query failed", ex);
		}

		// Pagefile
		try
		{
			context.ActionLogger.Info(area, "PageFile", "Start");
			foreach (var pf in context.Cim.Query(
				         "SELECT Name, AllocatedBaseSize, CurrentUsage, PeakUsage FROM Win32_PageFileUsage"))
				pagefile.Add(new
				{
					Name = pf.GetOrDefault("Name"), AllocatedBaseSize = pf.GetOrDefault("AllocatedBaseSize"),
					CurrentUsage = pf.GetOrDefault("CurrentUsage"), PeakUsage = pf.GetOrDefault("PeakUsage")
				});
			context.ActionLogger.Info(area, "PageFile", $"Complete: entries={pagefile.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"PageFile query failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "PageFile", "PageFile query failed", ex);
		}

		// Power plan (best-effort via registry)
		try
		{
			context.ActionLogger.Info(area, "Power", "Start");
			try
			{
				power["ActiveScheme"] = context.Registry.GetValue(
					"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power\\User\\PowerSchemes", "ActivePowerScheme");
			}
			catch
			{
			}

			context.ActionLogger.Info(area, "Power", "Complete");
		}
		catch (Exception ex)
		{
			warnings.Add($"Power plan read failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Power", "Power plan read failed", ex);
		}

		// Locale info (Control Panel International)
		try
		{
			context.ActionLogger.Info(area, "Locale", "Start");
			foreach (var name in new[] { "Locale", "LocaleName", "sShortDate", "sTimeFormat" })
				try
				{
					locale[name] = context.Registry.GetValue("HKCU\\Control Panel\\International", name) ??
					               string.Empty;
				}
				catch
				{
				}

			context.ActionLogger.Info(area, "Locale", "Complete");
		}
		catch (Exception ex)
		{
			warnings.Add($"Locale read failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "Locale", "Locale read failed", ex);
		}

		// Summary and result
		var summary = new
		{
			Name = os.GetValueOrDefault("Caption")?.ToString(),
			Build = os.GetValueOrDefault("BuildNumber")?.ToString(),
			Arch = os.GetValueOrDefault("OSArchitecture")?.ToString(),
			DomainJoin = system.GetValueOrDefault("PartOfDomain"),
			AutoStartServiceIssues = servicesAutoIssues.Count
		};
		var details = new
		{
			System = system,
			OS = os,
			BIOS = bios,
			Install = install,
			PendingReboot = pendingReboot,
			Services = services,
			Updates = updates,
			Time = timeInfo,
			PageFile = pagefile,
			Power = power,
			Locale = locale
		};
		var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), warnings, errors);
		context.ActionLogger.Info(area, "Complete", "OS and system information collected");

		return Task.FromResult(result);
	}





	private static bool TryParseDmtfDate(string dmtf, out DateTimeOffset utc)
	{
		utc = default;
		try
		{
			if (string.IsNullOrEmpty(dmtf) || dmtf.Length < 14) return false;

			var year = int.Parse(dmtf.Substring(0, 4));
			var month = int.Parse(dmtf.Substring(4, 2));
			var day = int.Parse(dmtf.Substring(6, 2));
			var hour = int.Parse(dmtf.Substring(8, 2));
			var minute = int.Parse(dmtf.Substring(10, 2));
			var second = int.Parse(dmtf.Substring(12, 2));
			var dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
			utc = new DateTimeOffset(dt).ToUniversalTime();

			return true;
		}
		catch
		{
			return false;
		}
	}





	private static bool KeyExists(IAnalyzerContext context, string key, string subKeyName)
	{
		try
		{
			foreach (var name in context.Registry.EnumerateSubKeys(key))
				if (string.Equals(name, subKeyName, StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}
		catch
		{
			return false;
		}
	}
}
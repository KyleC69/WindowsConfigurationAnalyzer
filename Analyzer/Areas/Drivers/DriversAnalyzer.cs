// Created:  2025/10/30
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Drivers;



public sealed class DriversAnalyzer : IAnalyzerModule
{
	public string Name => "Drivers Analyzer";
	public string Area => "Drivers";





	public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
	{
		var area = Area;
		context.ActionLogger.Info(area, "Start", "Collecting installed driver inventory");
		var warnings = new List<string>();
		var errors = new List<string>();

		var drivers = new List<object>();
		var problematic = new List<object>();
		try
		{
			context.ActionLogger.Info(area, "PnPDrivers", "Start");
			foreach (var d in context.Cim.Query(
				         "SELECT DeviceName, DriverVersion, DriverDate, Manufacturer, InfName, IsSigned, IsBootCritical, DriverProviderName, MatchingDeviceId, ClassName FROM Win32_PnPSignedDriver"))
			{
				var name = d.GetOrDefault("DeviceName")?.ToString();
				var ver = d.GetOrDefault("DriverVersion")?.ToString();
				var date = d.GetOrDefault("DriverDate")?.ToString();
				var mfg = d.GetOrDefault("Manufacturer")?.ToString();
				var inf = d.GetOrDefault("InfName")?.ToString();
				var signed = d.GetOrDefault("IsSigned");
				var bootCrit = d.GetOrDefault("IsBootCritical");
				var provider = d.GetOrDefault("DriverProviderName")?.ToString();
				var className = d.GetOrDefault("ClassName")?.ToString();
				var matchId = d.GetOrDefault("MatchingDeviceId")?.ToString();
				var entry = new
				{
					Name = name, Version = ver, Date = date, Manufacturer = mfg, Provider = provider, Class = className,
					Inf = inf, IsSigned = signed, IsBootCritical = bootCrit, MatchingDeviceId = matchId
				};
				drivers.Add(entry);
				if (signed is bool b && !b) problematic.Add(new { Reason = "Unsigned", Item = entry });
			}

			context.ActionLogger.Info(area, "PnPDrivers",
				$"Complete: count={drivers.Count}, issues={problematic.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"PnP driver query failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "PnPDrivers", "PnP driver query failed", ex);
		}

		// Service-based drivers (Start type, error control)
		var serviceDrivers = new List<object>();
		try
		{
			context.ActionLogger.Info(area, "ServiceDrivers", "Start");
			foreach (var s in context.Cim.Query(
				         "SELECT Name, StartMode, State, PathName, ServiceType, ErrorControl, DisplayName FROM Win32_Service WHERE ServiceType LIKE '%Kernel Driver%' OR ServiceType LIKE '%File System Driver%'"))
				serviceDrivers.Add(new
				{
					Name = s.GetOrDefault("Name"), State = s.GetOrDefault("State"),
					StartMode = s.GetOrDefault("StartMode"), ErrorControl = s.GetOrDefault("ErrorControl"),
					PathName = s.GetOrDefault("PathName"), DisplayName = s.GetOrDefault("DisplayName")
				});
			context.ActionLogger.Info(area, "ServiceDrivers", $"Complete: count={serviceDrivers.Count}");
		}
		catch (Exception ex)
		{
			warnings.Add($"Service driver query failed: {ex.Message}");
			errors.Add(ex.ToString());
			context.ActionLogger.Error(area, "ServiceDrivers", "Service driver query failed", ex);
		}

		var summary = new
			{ Drivers = drivers.Count, ServiceDrivers = serviceDrivers.Count, Problematic = problematic.Count };
		var details = new { PnPSignedDrivers = drivers, ServiceDrivers = serviceDrivers, Problematic = problematic };
		var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), warnings, errors);
		context.ActionLogger.Info(area, "Complete", "Driver inventory collected");

		return Task.FromResult(result);
	}
}
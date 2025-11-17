// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  DriversAnalyzer.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Drivers;


public sealed class DriversAnalyzer : IAnalyzerModule
{


    public string Name => "Drivers Analyzer";
    public string Area => "Drivers";

    private IActivityLogger? _logger;




    /// <summary>
    /// Analyzes the drivers in the system and collects information about installed drivers, 
    /// service drivers, and any problematic drivers.
    /// </summary>
    /// <param name="logger">The logger used to log activity during the analysis.</param>
    /// <param name="context">The context providing access to system resources for analysis.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="AreaResult"/> containing the analysis results, including a summary, 
    /// details of drivers, warnings, and errors encountered during the analysis.
    /// </returns>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled.</exception>
    /// <exception cref="Exception">Thrown if an unexpected error occurs during the analysis.</exception>
    public async Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger;
        var area = Area;
        _logger.Log("INF", "{area} Start Collecting installed driver inventory", $"{area}");
        List<string> warnings = new();
        List<string> errors = new();

        List<object> drivers = new();
        List<object> problematic = new();
        try
        {
            _logger.Log("INF", "PnPDrivers", "Start");
            var pnpRows = await context.Cim.QueryAsync(
                "SELECT DeviceName, DriverVersion, DriverDate, Manufacturer, InfName, IsSigned, IsBootCritical, DriverProviderName, MatchingDeviceId, ClassName FROM Win32_PnPSignedDriver",
                null, cancellationToken).ConfigureAwait(false);
            foreach (var d in pnpRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entry = new
                {
                    Name = d.GetOrDefault("DeviceName")?.ToString(),
                    Version = d.GetOrDefault("DriverVersion")?.ToString(),
                    Date = d.GetOrDefault("DriverDate")?.ToString(),
                    Manufacturer = d.GetOrDefault("Manufacturer")?.ToString(),
                    Provider = d.GetOrDefault("DriverProviderName")?.ToString(),
                    Class = d.GetOrDefault("ClassName")?.ToString(),
                    Inf = d.GetOrDefault("InfName")?.ToString(),
                    IsSigned = d.GetOrDefault("IsSigned"),
                    IsBootCritical = d.GetOrDefault("IsBootCritical"),
                    MatchingDeviceId = d.GetOrDefault("MatchingDeviceId")?.ToString()
                };
                drivers.Add(entry);
                if (d.GetOrDefault("IsSigned") is bool b && !b)
                {
                    problematic.Add(new { Reason = "Unsigned", Item = entry });
                }
            }
            _logger.Log(area, "PnPDrivers", $"Complete: count={drivers.Count}, issues={problematic.Count}");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            warnings.Add($"PnP driver query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", "PnP driver query failed", area);
        }

        List<object> serviceDrivers = new();
        try
        {
            _logger.Log("INF", "ServiceDrivers", area);
            var svcRows = await context.Cim.QueryAsync(
                "SELECT Name, StartMode, State, PathName, ServiceType, ErrorControl, DisplayName FROM Win32_Service WHERE ServiceType LIKE '%Kernel Driver%' OR ServiceType LIKE '%File System Driver%'",
                null, cancellationToken).ConfigureAwait(false);
            foreach (var s in svcRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                serviceDrivers.Add(new
                {
                    Name = s.GetOrDefault("Name"),
                    State = s.GetOrDefault("State"),
                    StartMode = s.GetOrDefault("StartMode"),
                    ErrorControl = s.GetOrDefault("ErrorControl"),
                    PathName = s.GetOrDefault("PathName"),
                    DisplayName = s.GetOrDefault("DisplayName")
                });
            }
            _logger.Log("INF", $"Complete: count={serviceDrivers.Count}", area);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            warnings.Add($"Service driver query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", "Service driver query failed", area);
        }

        var summary = new { Drivers = drivers.Count, ServiceDrivers = serviceDrivers.Count, Problematic = problematic.Count };
        var details = new { PnPSignedDrivers = drivers, ServiceDrivers = serviceDrivers, Problematic = problematic };
        AreaResult result = new(area, summary, details, new List<Finding>().AsReadOnly(), warnings, errors);
        _logger.Log("INF", "Driver inventory collected", area);
        return result;
    }


}
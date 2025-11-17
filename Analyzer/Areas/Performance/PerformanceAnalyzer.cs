// Created:  2025/10/30
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  PerformanceAnalyzer.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Performance;


public sealed class PerformanceAnalyzer : IAnalyzerModule
{


    private IActivityLogger? _logger;


    public string Name => "Performance Analyzer";
    public string Area => "Performance";



    public async Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger;
        var area = Area;
        _logger.Log("INF", "Start: Collecting performance metrics", area);
        List<string> warnings = new();
        List<string> errors = new();

        double cpuPct = -1;
        double memPct = -1;
        TimeSpan? uptime = null;

        List<Dictionary<string, object?>> cpuAll = new();
        Dictionary<string, object?> memCounters = new();
        Dictionary<string, object?> systemCounters = new();
        List<Dictionary<string, object?>> disks = new();
        List<Dictionary<string, object?>> pagingFiles = new();
        List<Dictionary<string, object?>> topProcCpu = new();
        List<Dictionary<string, object?>> topProcMem = new();

        try
        {
            _logger.Log("INF", "Uptime: Start", area);
            var osRows = await context.Cim.QueryAsync(
                "SELECT LastBootUpTime, TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem",
                null, cancellationToken).ConfigureAwait(false);
            foreach (var os in osRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var lastBoot = os.GetOrDefault("LastBootUpTime");
                if (lastBoot is string lb && TryParseDmtfDate(lb, out DateTimeOffset bootUtc))
                {
                    uptime = DateTimeOffset.UtcNow - bootUtc;
                }
                var total = ToDouble(os.GetOrDefault("TotalVisibleMemorySize"));
                var free = ToDouble(os.GetOrDefault("FreePhysicalMemory"));
                if (total > 0)
                {
                    var used = total - free;
                    memPct = Math.Round(used / total * 100.0, 2);
                }
                break;
            }
            _logger.Log("INF", $"Uptime: Complete: uptime={uptime?.ToString() ?? "n/a"}, mem%={memPct}", area);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            warnings.Add($"OS performance query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Uptime: OS performance query failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "CPU: Start", area);
            var cpuRows = await context.Cim.QueryAsync(
                "SELECT Name, PercentProcessorTime, PercentPrivilegedTime, PercentUserTime, InterruptsPerSec, DPCsQueuedPerSec FROM Win32_PerfFormattedData_PerfOS_Processor",
                null, cancellationToken).ConfigureAwait(false);
            foreach (var row in cpuRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = row.GetOrDefault("Name")?.ToString() ?? string.Empty;
                var total = ToDouble(row.GetOrDefault("PercentProcessorTime"));
                var kernel = ToDouble(row.GetOrDefault("PercentPrivilegedTime"));
                var user = ToDouble(row.GetOrDefault("PercentUserTime"));
                var interrupts = ToDouble(row.GetOrDefault("InterruptsPerSec"));
                var dpcs = ToDouble(row.GetOrDefault("DPCsQueuedPerSec"));
                Dictionary<string, object?> item = new()
                {
                    ["Name"] = name,
                    ["PercentProcessorTime"] = total,
                    ["PercentPrivilegedTime"] = kernel,
                    ["PercentUserTime"] = user,
                    ["InterruptsPerSec"] = interrupts,
                    ["DPCsQueuedPerSec"] = dpcs
                };
                if (string.Equals(name, "_Total", StringComparison.OrdinalIgnoreCase))
                {
                    cpuPct = total;
                }

                cpuAll.Add(item);
            }
            _logger.Log("INF", $"CPU: Complete: cpu%={cpuPct}, cores={cpuAll.Count}", area);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            warnings.Add($"CPU counter query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"CPU: CPU counter query failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "Memory: Start", area);
            var memRows = await context.Cim.QueryAsync(
                "SELECT AvailableMBytes, CacheBytes, CommittedBytes, PoolPagedBytes, PoolNonpagedBytes, PagesPerSec FROM Win32_PerfFormattedData_PerfOS_Memory",
                null, cancellationToken).ConfigureAwait(false);
            foreach (var row in memRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                memCounters["AvailableMBytes"] = ToDouble(row.GetOrDefault("AvailableMBytes"));
                memCounters["CacheBytes"] = ToDouble(row.GetOrDefault("CacheBytes"));
                memCounters["CommittedBytes"] = ToDouble(row.GetOrDefault("CommittedBytes"));
                memCounters["PoolPagedBytes"] = ToDouble(row.GetOrDefault("PoolPagedBytes"));
                memCounters["PoolNonpagedBytes"] = ToDouble(row.GetOrDefault("PoolNonpagedBytes"));
                memCounters["PagesPerSec"] = ToDouble(row.GetOrDefault("PagesPerSec"));
                break;
            }
            _logger.Log("INF", "Memory: Complete", area);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            warnings.Add($"Memory counter query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Memory: Memory counter query failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "System: Start", area);
            var sysRows = await context.Cim.QueryAsync(
                "SELECT Processes, Threads, ContextSwitchesPerSec, ProcessorQueueLength FROM Win32_PerfFormattedData_PerfOS_System",
                null, cancellationToken).ConfigureAwait(false);
            foreach (var row in sysRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                systemCounters["Processes"] = ToDouble(row.GetOrDefault("Processes"));
                systemCounters["Threads"] = ToDouble(row.GetOrDefault("Threads"));
                systemCounters["ContextSwitchesPerSec"] = ToDouble(row.GetOrDefault("ContextSwitchesPerSec"));
                systemCounters["ProcessorQueueLength"] = ToDouble(row.GetOrDefault("ProcessorQueueLength"));
                break;
            }
            _logger.Log("INF", "System: Complete", area);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            warnings.Add($"System counter query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"System: System counter query failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "Disk: Start", area);
            var diskRows = await context.Cim.QueryAsync(
                "SELECT Name, DiskReadsPerSec, DiskWritesPerSec, AvgDiskQueueLength, CurrentDiskQueueLength, PercentDiskTime, AvgDisksecPerRead, AvgDisksecPerWrite FROM Win32_PerfFormattedData_PerfDisk_PhysicalDisk",
                null, cancellationToken).ConfigureAwait(false);
            foreach (var row in diskRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                disks.Add(new Dictionary<string, object?>
                {
                    ["Name"] = row.GetOrDefault("Name")?.ToString(),
                    ["DiskReadsPerSec"] = ToDouble(row.GetOrDefault("DiskReadsPerSec")),
                    ["DiskWritesPerSec"] = ToDouble(row.GetOrDefault("DiskWritesPerSec")),
                    ["AvgDiskQueueLength"] = ToDouble(row.GetOrDefault("AvgDiskQueueLength")),
                    ["CurrentDiskQueueLength"] = ToDouble(row.GetOrDefault("CurrentDiskQueueLength")),
                    ["PercentDiskTime"] = ToDouble(row.GetOrDefault("PercentDiskTime")),
                    ["AvgDisksecPerRead"] = ToDouble(row.GetOrDefault("AvgDisksecPerRead")),
                    ["AvgDisksecPerWrite"] = ToDouble(row.GetOrDefault("AvgDisksecPerWrite"))
                });
            }
            _logger.Log("INF", $"Disk: Complete: disks={disks.Count}", area);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            warnings.Add($"Disk counter query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Disk: Disk counter query failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "PagingFile: Start", area);
            var pfRows = await context.Cim.QueryAsync(
                "SELECT Name, PercentUsage FROM Win32_PerfFormattedData_PerfOS_PagingFile",
                null, cancellationToken).ConfigureAwait(false);
            foreach (var row in pfRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                pagingFiles.Add(new Dictionary<string, object?>
                {
                    ["Name"] = row.GetOrDefault("Name")?.ToString(),
                    ["PercentUsage"] = ToDouble(row.GetOrDefault("PercentUsage"))
                });
            }
            _logger.Log("INF", "PagingFile: Complete", area);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            warnings.Add($"Paging file query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"PagingFile: Paging file query failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "TopProcesses: Start", area);
            var procRows = await context.Cim.QueryAsync(
                "SELECT IDProcess, Name, PercentProcessorTime, WorkingSetPrivate, ElapsedTime FROM Win32_PerfFormattedData_PerfProc_Process",
                null, cancellationToken).ConfigureAwait(false);
            List<Dictionary<string, object?>> proc = new();
            foreach (var row in procRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = row.GetOrDefault("Name")?.ToString();
                if (string.Equals(name, "_Total", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                proc.Add(new Dictionary<string, object?>
                {
                    ["ProcessId"] = ToInt(row.GetOrDefault("IDProcess")),
                    ["Name"] = name,
                    ["PercentProcessorTime"] = ToDouble(row.GetOrDefault("PercentProcessorTime")),
                    ["WorkingSetPrivate"] = ToDouble(row.GetOrDefault("WorkingSetPrivate")),
                    ["ElapsedTimeSec"] = ToDouble(row.GetOrDefault("ElapsedTime"))
                });
            }
            topProcCpu = proc.OrderByDescending(p => ToDouble(p.GetValueOrDefault("PercentProcessorTime"))).Take(10).ToList();
            topProcMem = proc.OrderByDescending(p => ToDouble(p.GetValueOrDefault("WorkingSetPrivate"))).Take(10).ToList();
            _logger.Log("INF", $"TopProcesses: Complete: topCpu={topProcCpu.Count}, topMem={topProcMem.Count}", area);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            warnings.Add($"Process counters query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"TopProcesses: Process counters query failed ({ex.Message})", area);
        }

        var summary = new { CpuPercent = cpuPct, MemoryUsedPercent = memPct, Uptime = uptime?.ToString() };
        Dictionary<string, object?> details = new()
        {
            ["CpuPercent"] = cpuPct,
            ["MemoryUsedPercent"] = memPct,
            ["Uptime"] = uptime?.ToString(),
            ["Cpu"] = cpuAll,
            ["Memory"] = memCounters,
            ["System"] = systemCounters,
            ["Disks"] = disks,
            ["PagingFile"] = pagingFiles,
            ["TopProcessesCpu"] = topProcCpu,
            ["TopProcessesMemory"] = topProcMem
        };
        AreaResult result = new(area, summary, details, new List<Finding>().AsReadOnly(), warnings, errors);
        _logger.Log("INF", "Complete: Performance metrics collected", area);

        return result;
    }





    private static bool TryParseDmtfDate(string dmtf, out DateTimeOffset utc)
    {
        // Basic DMTF WMI datetime parser (yyyyMMddHHmmss.mmmmmmsUUU)
        utc = default;
        try
        {
            if (dmtf.Length < 25)
            {
                return false;
            }

            var year = int.Parse(dmtf.Substring(0, 4));
            var month = int.Parse(dmtf.Substring(4, 2));
            var day = int.Parse(dmtf.Substring(6, 2));
            var hour = int.Parse(dmtf.Substring(8, 2));
            var minute = int.Parse(dmtf.Substring(10, 2));
            var second = int.Parse(dmtf.Substring(12, 2));
            DateTime dt = new(year, month, day, hour, minute, second, DateTimeKind.Local);
            utc = new DateTimeOffset(dt).ToUniversalTime();
            return true;
        }
        catch { return false; }
    }



    private static double ToDouble(object? v)
    {
        if (v is null)
        {
            return 0d;
        }

        try { return Convert.ToDouble(v); } catch { return double.TryParse(v.ToString(), out var d) ? d : 0d; }
    }



    private static int ToInt(object? v)
    {
        if (v is null)
        {
            return 0;
        }

        try { return Convert.ToInt32(v); } catch { return int.TryParse(v.ToString(), out var i) ? i : 0; }
    }


}
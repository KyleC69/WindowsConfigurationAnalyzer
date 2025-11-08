// Created:  2025/10/30
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  PerformanceAnalyzer.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Performance;



public sealed class PerformanceAnalyzer : IAnalyzerModule
{
    public string Name => "Performance Analyzer";
    public string Area => "Performance";





    public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
    {
        var area = Area;
        context.ActionLogger.Info(area, "Start", "Collecting performance metrics");
        List<string> warnings = new();
        List<string> errors = new();

        double cpuPct = -1;
        double memPct = -1;
        TimeSpan? uptime = null;

        // Containers for detailed results
        List<Dictionary<string, object?>> cpuAll = new();
        Dictionary<string, object?> memCounters = new();
        Dictionary<string, object?> systemCounters = new();
        List<Dictionary<string, object?>> disks = new();
        List<Dictionary<string, object?>> pagingFiles = new();
        List<Dictionary<string, object?>> topProcCpu = new();
        List<Dictionary<string, object?>> topProcMem = new();

        // Uptime + base memory percent
        try
        {
            context.ActionLogger.Info(area, "Uptime", "Start");
            foreach (var os in context.Cim.Query(
                         "SELECT LastBootUpTime, TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
            {
                var lastBoot = os.GetOrDefault("LastBootUpTime");
                if (lastBoot is string lb && TryParseDmtfDate(lb, out var bootUtc))
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

            context.ActionLogger.Info(area, "Uptime", $"Complete: uptime={uptime?.ToString() ?? "n/a"}, mem%={memPct}");
        }
        catch (Exception ex)
        {
            warnings.Add($"OS performance query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Uptime", "OS performance query failed", ex);
        }

        // CPU total and per-core
        try
        {
            context.ActionLogger.Info(area, "CPU", "Start");
            foreach (var row in context.Cim.Query(
                         "SELECT Name, PercentProcessorTime, PercentPrivilegedTime, PercentUserTime, InterruptsPerSec, DPCsQueuedPerSec FROM Win32_PerfFormattedData_PerfOS_Processor"))
            {
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

            context.ActionLogger.Info(area, "CPU", $"Complete: cpu%={cpuPct}, cores={cpuAll.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"CPU counter query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "CPU", "CPU counter query failed", ex);
        }

        // Memory counters (PerfOS_Memory)
        try
        {
            context.ActionLogger.Info(area, "Memory", "Start");
            foreach (var row in context.Cim.Query(
                         "SELECT AvailableMBytes, CacheBytes, CommittedBytes, PoolPagedBytes, PoolNonpagedBytes, PagesPerSec FROM Win32_PerfFormattedData_PerfOS_Memory"))
            {
                memCounters["AvailableMBytes"] = ToDouble(row.GetOrDefault("AvailableMBytes"));
                memCounters["CacheBytes"] = ToDouble(row.GetOrDefault("CacheBytes"));
                memCounters["CommittedBytes"] = ToDouble(row.GetOrDefault("CommittedBytes"));
                memCounters["PoolPagedBytes"] = ToDouble(row.GetOrDefault("PoolPagedBytes"));
                memCounters["PoolNonpagedBytes"] = ToDouble(row.GetOrDefault("PoolNonpagedBytes"));
                memCounters["PagesPerSec"] = ToDouble(row.GetOrDefault("PagesPerSec"));

                break;
            }

            context.ActionLogger.Info(area, "Memory", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"Memory counter query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Memory", "Memory counter query failed", ex);
        }

        // System counters
        try
        {
            context.ActionLogger.Info(area, "System", "Start");
            foreach (var row in context.Cim.Query(
                         "SELECT Processes, Threads, ContextSwitchesPerSec, ProcessorQueueLength FROM Win32_PerfFormattedData_PerfOS_System"))
            {
                systemCounters["Processes"] = ToDouble(row.GetOrDefault("Processes"));
                systemCounters["Threads"] = ToDouble(row.GetOrDefault("Threads"));
                systemCounters["ContextSwitchesPerSec"] = ToDouble(row.GetOrDefault("ContextSwitchesPerSec"));
                systemCounters["ProcessorQueueLength"] = ToDouble(row.GetOrDefault("ProcessorQueueLength"));

                break;
            }

            context.ActionLogger.Info(area, "System", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"System counter query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "System", "System counter query failed", ex);
        }

        // Disk (PhysicalDisk)
        try
        {
            context.ActionLogger.Info(area, "Disk", "Start");
            foreach (var row in context.Cim.Query(
                         "SELECT Name, DiskReadsPerSec, DiskWritesPerSec, AvgDiskQueueLength, CurrentDiskQueueLength, PercentDiskTime, AvgDisksecPerRead, AvgDisksecPerWrite FROM Win32_PerfFormattedData_PerfDisk_PhysicalDisk"))
            {
                Dictionary<string, object?> item = new()
                {
                    ["Name"] = row.GetOrDefault("Name")?.ToString(),
                    ["DiskReadsPerSec"] = ToDouble(row.GetOrDefault("DiskReadsPerSec")),
                    ["DiskWritesPerSec"] = ToDouble(row.GetOrDefault("DiskWritesPerSec")),
                    ["AvgDiskQueueLength"] = ToDouble(row.GetOrDefault("AvgDiskQueueLength")),
                    ["CurrentDiskQueueLength"] = ToDouble(row.GetOrDefault("CurrentDiskQueueLength")),
                    ["PercentDiskTime"] = ToDouble(row.GetOrDefault("PercentDiskTime")),
                    ["AvgDisksecPerRead"] = ToDouble(row.GetOrDefault("AvgDisksecPerRead")),
                    ["AvgDisksecPerWrite"] = ToDouble(row.GetOrDefault("AvgDisksecPerWrite"))
                };
                disks.Add(item);
            }

            context.ActionLogger.Info(area, "Disk", $"Complete: disks={disks.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Disk counter query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Disk", "Disk counter query failed", ex);
        }

        // Paging file usage
        try
        {
            context.ActionLogger.Info(area, "PagingFile", "Start");
            foreach (var row in context.Cim.Query(
                         "SELECT Name, PercentUsage FROM Win32_PerfFormattedData_PerfOS_PagingFile"))
            {
                Dictionary<string, object?> item = new()
                {
                    ["Name"] = row.GetOrDefault("Name")?.ToString(),
                    ["PercentUsage"] = ToDouble(row.GetOrDefault("PercentUsage"))
                };
                pagingFiles.Add(item);
            }

            context.ActionLogger.Info(area, "PagingFile", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"Paging file query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "PagingFile", "Paging file query failed", ex);
        }

        // Top processes by CPU and Memory
        try
        {
            context.ActionLogger.Info(area, "TopProcesses", "Start");
            List<Dictionary<string, object?>> proc = new();
            foreach (var row in context.Cim.Query(
                         "SELECT IDProcess, Name, PercentProcessorTime, WorkingSetPrivate, ElapsedTime FROM Win32_PerfFormattedData_PerfProc_Process"))
            {
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

            topProcCpu = proc.OrderByDescending(p => ToDouble(p.GetValueOrDefault("PercentProcessorTime"))).Take(10)
                .ToList();
            topProcMem = proc.OrderByDescending(p => ToDouble(p.GetValueOrDefault("WorkingSetPrivate"))).Take(10)
                .ToList();
            context.ActionLogger.Info(area, "TopProcesses",
                $"Complete: topCpu={topProcCpu.Count}, topMem={topProcMem.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Process counters query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "TopProcesses", "Process counters query failed", ex);
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
        AreaResult result = new(area, summary, details, Array.Empty<Finding>(), warnings, errors);
        context.ActionLogger.Info(area, "Complete", "Performance metrics collected");

        return Task.FromResult(result);
    }





    private static bool TryParseDmtfDate(string dmtf, out DateTimeOffset utc)
    {
        // Basic DMTF WMI datetime parser (yyyyMMddHHmmss.mmmmmmsUUU)
        utc = default(DateTimeOffset);
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
            // ignore microseconds and UTC offset for simplicity
            DateTime dt = new(year, month, day, hour, minute, second, DateTimeKind.Local);
            utc = new DateTimeOffset(dt).ToUniversalTime();

            return true;
        }
        catch
        {
            return false;
        }
    }





    private static double ToDouble(object? v)
    {
        if (v is null)
        {
            return 0d;
        }

        try
        {
            return Convert.ToDouble(v);
        }
        catch
        {
            return double.TryParse(v.ToString(), out var d) ? d : 0d;
        }
    }





    private static int ToInt(object? v)
    {
        if (v is null)
        {
            return 0;
        }

        try
        {
            return Convert.ToInt32(v);
        }
        catch
        {
            return int.TryParse(v.ToString(), out var i) ? i : 0;
        }
    }
}
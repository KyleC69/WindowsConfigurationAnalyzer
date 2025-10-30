using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Hardware;

public sealed class HardwareAnalyzer : IAnalyzerModule
{
    public string Name => "Hardware Analyzer";
    public string Area => "Hardware";

    public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
    {
        var area = Area;
        context.ActionLogger.Info(area, "Collect", "Collecting hardware inventory via CIM");

        var cpuList = new List<object>();
        var disks = new List<object>();
        double totalMemGb = 0;
        try
        {
            foreach (var mo in context.Cim.Query("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
            {
                cpuList.Add(new
                {
                    Name = mo.GetOrDefault("Name"),
                    Cores = Convert.ToInt32(mo.GetOrDefault("NumberOfCores") ?? 0),
                    LogicalProcessors = Convert.ToInt32(mo.GetOrDefault("NumberOfLogicalProcessors") ?? 0)
                });
            }

            foreach (var mo in context.Cim.Query("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
            {
                var bytes = mo.GetOrDefault("TotalPhysicalMemory");
                if (bytes is ulong ul)
                {
                    totalMemGb = Math.Round(ul / 1024d / 1024d / 1024d, 2);
                }
            }

            foreach (var mo in context.Cim.Query("SELECT Index, Model, Size, MediaType FROM Win32_DiskDrive"))
            {
                var s = mo.GetOrDefault("Size");
                var size = s is ulong sz ? Math.Round(sz / 1024d / 1024d / 1024d, 2) : 0d;
                disks.Add(new
                {
                    Index = Convert.ToInt32(mo.GetOrDefault("Index") ?? 0),
                    Model = mo.GetOrDefault("Model"),
                    SizeGB = size,
                    MediaType = mo.GetOrDefault("MediaType")
                });
            }
        }
        catch (Exception ex)
        {
            context.ActionLogger.Warn(area, "Collect", $"CIM query failed: {ex.Message}");
        }

        var summary = new { CPUCount = cpuList.Count, MemoryGB = totalMemGb, DiskCount = disks.Count };
        var details = new { CPUs = cpuList, MemoryGB = totalMemGb, Disks = disks };
        var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), Array.Empty<string>(), Array.Empty<string>());
        context.ActionLogger.Info(area, "Collect", "Hardware inventory collected");
        return Task.FromResult(result);
    }
}

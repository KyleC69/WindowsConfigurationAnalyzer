// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  HardwareAnalyzer.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



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
        context.ActionLogger.Info(area, "Start", "Collecting hardware inventory via CIM");
        List<string> warnings = new();
        List<string> errors = new();

        List<object> cpuList = new();
        List<object> memoryModules = new();
        List<object> disks = new();
        List<object> partitions = new();
        List<object> volumes = new();
        List<object> gpu = new();
        Dictionary<string, object?> baseboard = new();
        Dictionary<string, object?> enclosure = new();
        Dictionary<string, object?> tpm = new();
        List<object> battery = new();
        double totalMemGb = 0;

        // CPU
        try
        {
            context.ActionLogger.Info(area, "CPU", "Start");
            foreach (var mo in context.Cim.Query(
                         "SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, ProcessorId, SecondLevelAddressTranslationExtensions, VirtualizationFirmwareEnabled FROM Win32_Processor"))
            {
                cpuList.Add(new
                {
                    Name = mo.GetOrDefault("Name"),
                    Cores = Convert.ToInt32(mo.GetOrDefault("NumberOfCores") ?? 0),
                    LogicalProcessors = Convert.ToInt32(mo.GetOrDefault("NumberOfLogicalProcessors") ?? 0),
                    MaxClockMHz = mo.GetOrDefault("MaxClockSpeed"),
                    ProcessorId = mo.GetOrDefault("ProcessorId"),
                    SLAT = mo.GetOrDefault("SecondLevelAddressTranslationExtensions"),
                    VTEnabled = mo.GetOrDefault("VirtualizationFirmwareEnabled")
                });
            }

            context.ActionLogger.Info(area, "CPU", $"Complete: count={cpuList.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"CPU query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "CPU", "CPU query failed", ex);
        }

        // Memory total and modules
        try
        {
            context.ActionLogger.Info(area, "Memory", "Start");
            foreach (var mo in context.Cim.Query(
                         "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
            {
                var bytes = mo.GetOrDefault("TotalPhysicalMemory");
                if (bytes is ulong ul)
                {
                    totalMemGb = Math.Round(ul / 1024d / 1024d / 1024d, 2);
                }

                break;
            }

            foreach (var dimm in context.Cim.Query(
                         "SELECT BankLabel, Capacity, DeviceLocator, Manufacturer, PartNumber, SerialNumber, Speed FROM Win32_PhysicalMemory"))
            {
                memoryModules.Add(new
                {
                    Bank = dimm.GetOrDefault("BankLabel"),
                    CapacityGB = ToGb(dimm.GetOrDefault("Capacity")),
                    DeviceLocator = dimm.GetOrDefault("DeviceLocator"),
                    Manufacturer = dimm.GetOrDefault("Manufacturer"),
                    PartNumber = dimm.GetOrDefault("PartNumber"),
                    SerialNumber = dimm.GetOrDefault("SerialNumber"),
                    SpeedMHz = dimm.GetOrDefault("Speed")
                });
            }

            context.ActionLogger.Info(area, "Memory", $"Complete: total={totalMemGb}GB, modules={memoryModules.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Memory inventory failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Memory", "Memory inventory failed", ex);
        }

        // Disks, partitions, volumes
        try
        {
            context.ActionLogger.Info(area, "Disks", "Start");
            foreach (var mo in context.Cim.Query(
                         "SELECT Index, Model, Size, MediaType, SerialNumber, InterfaceType FROM Win32_DiskDrive"))
            {
                var s = mo.GetOrDefault("Size");
                var sizeGb = s is ulong sz ? Math.Round(sz / 1024d / 1024d / 1024d, 2) : 0d;
                disks.Add(new
                {
                    Index = Convert.ToInt32(mo.GetOrDefault("Index") ?? 0),
                    Model = mo.GetOrDefault("Model"),
                    SizeGB = sizeGb,
                    MediaType = mo.GetOrDefault("MediaType"),
                    SerialNumber = mo.GetOrDefault("SerialNumber"),
                    InterfaceType = mo.GetOrDefault("InterfaceType")
                });
            }

            foreach (var p in context.Cim.Query(
                         "SELECT DeviceID, DiskIndex, Index, Type, BootPartition, PrimaryPartition, Size FROM Win32_DiskPartition"))
            {
                partitions.Add(new
                {
                    DeviceID = p.GetOrDefault("DeviceID"),
                    DiskIndex = p.GetOrDefault("DiskIndex"),
                    Index = p.GetOrDefault("Index"),
                    Type = p.GetOrDefault("Type"),
                    Boot = p.GetOrDefault("BootPartition"),
                    Primary = p.GetOrDefault("PrimaryPartition"),
                    SizeGB = ToGb(p.GetOrDefault("Size"))
                });
            }

            foreach (var v in context.Cim.Query(
                         "SELECT DeviceID, VolumeName, FileSystem, Size, FreeSpace, DriveType FROM Win32_LogicalDisk WHERE DriveType=3"))
            {
                volumes.Add(new
                {
                    DeviceID = v.GetOrDefault("DeviceID"),
                    Label = v.GetOrDefault("VolumeName"),
                    FileSystem = v.GetOrDefault("FileSystem"),
                    SizeGB = ToGb(v.GetOrDefault("Size")),
                    FreeGB = ToGb(v.GetOrDefault("FreeSpace")),
                    DriveType = v.GetOrDefault("DriveType")
                });
            }

            context.ActionLogger.Info(area, "Disks",
                $"Complete: disks={disks.Count}, partitions={partitions.Count}, volumes={volumes.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Disk inventory failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Disks", "Disk inventory failed", ex);
        }

        // GPU
        try
        {
            context.ActionLogger.Info(area, "GPU", "Start");
            foreach (var vc in context.Cim.Query(
                         "SELECT Name, AdapterCompatibility, DriverVersion, DriverDate, AdapterRAM, VideoProcessor FROM Win32_VideoController"))
            {
                gpu.Add(new
                {
                    Name = vc.GetOrDefault("Name"),
                    Vendor = vc.GetOrDefault("AdapterCompatibility"),
                    DriverVersion = vc.GetOrDefault("DriverVersion"),
                    DriverDate = vc.GetOrDefault("DriverDate"),
                    AdapterRAM = vc.GetOrDefault("AdapterRAM"),
                    VideoProcessor = vc.GetOrDefault("VideoProcessor")
                });
            }

            context.ActionLogger.Info(area, "GPU", $"Complete: count={gpu.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"GPU query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "GPU", "GPU query failed", ex);
        }

        // Baseboard and system enclosure
        try
        {
            context.ActionLogger.Info(area, "Board", "Start");
            foreach (var bb in context.Cim.Query(
                         "SELECT Manufacturer, Product, SerialNumber, Version FROM Win32_BaseBoard"))
            {
                baseboard["Manufacturer"] = bb.GetOrDefault("Manufacturer");
                baseboard["Product"] = bb.GetOrDefault("Product");
                baseboard["SerialNumber"] = bb.GetOrDefault("SerialNumber");
                baseboard["Version"] = bb.GetOrDefault("Version");

                break;
            }

            foreach (var enc in context.Cim.Query(
                         "SELECT ChassisTypes, Manufacturer, SMBIOSAssetTag FROM Win32_SystemEnclosure"))
            {
                enclosure["ChassisTypes"] = enc.GetOrDefault("ChassisTypes");
                enclosure["Manufacturer"] = enc.GetOrDefault("Manufacturer");
                enclosure["AssetTag"] = enc.GetOrDefault("SMBIOSAssetTag");

                break;
            }

            context.ActionLogger.Info(area, "Board", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"Baseboard/enclosure query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Board", "Baseboard/enclosure query failed", ex);
        }

        // TPM (best-effort)
        var tpmPresent = false;
        try
        {
            context.ActionLogger.Info(area, "TPM", "Start");
            foreach (var t in context.Cim.Query(
                         "SELECT IsEnabled_InitialValue, IsActivated_InitialValue, SpecVersion, ManufacturerIdTxt, ManufacturerVersion FROM Win32_Tpm",
                         "\\\\.\\root\\CIMV2\\Security\\MicrosoftTpm"))
            {
                tpmPresent = true;
                tpm["IsEnabled_InitialValue"] = t.GetOrDefault("IsEnabled_InitialValue");
                tpm["IsActivated_InitialValue"] = t.GetOrDefault("IsActivated_InitialValue");
                tpm["SpecVersion"] = t.GetOrDefault("SpecVersion");
                tpm["ManufacturerIdTxt"] = t.GetOrDefault("ManufacturerIdTxt");
                tpm["ManufacturerVersion"] = t.GetOrDefault("ManufacturerVersion");

                break;
            }

            context.ActionLogger.Info(area, "TPM", $"Complete: present={tpmPresent}");
        }
        catch (Exception ex)
        {
            warnings.Add($"TPM query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "TPM", "TPM query failed", ex);
        }

        // Battery (mobile devices)
        try
        {
            context.ActionLogger.Info(area, "Battery", "Start");
            foreach (var b in context.Cim.Query(
                         "SELECT Name, BatteryStatus, EstimatedChargeRemaining, EstimatedRunTime FROM Win32_Battery"))
            {
                battery.Add(new
                {
                    Name = b.GetOrDefault("Name"),
                    Status = b.GetOrDefault("BatteryStatus"),
                    ChargePercent = b.GetOrDefault("EstimatedChargeRemaining"),
                    EstimatedRunTimeMin = b.GetOrDefault("EstimatedRunTime")
                });
            }

            context.ActionLogger.Info(area, "Battery", $"Complete: count={battery.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Battery query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Battery", "Battery query failed", ex);
        }

        var summary = new
        {
            CPUCount = cpuList.Count,
            MemoryGB = totalMemGb,
            DiskCount = disks.Count,
            GPUCount = gpu.Count,
            TpmPresent = tpmPresent
        };
        var details = new
        {
            CPUs = cpuList,
            MemoryGB = totalMemGb,
            MemoryModules = memoryModules,
            Disks = disks,
            Partitions = partitions,
            Volumes = volumes,
            GPU = gpu,
            BaseBoard = baseboard,
            Enclosure = enclosure,
            TPM = tpm,
            Battery = battery
        };
        AreaResult result = new(area, summary, details, Array.Empty<Finding>(), warnings, errors);
        context.ActionLogger.Info(area, "Complete", "Hardware inventory collected");

        return Task.FromResult(result);
    }





    private static double ToGb(object? v)
    {
        try
        {
            return v is ulong ul
                ? Math.Round(ul / 1024d / 1024d / 1024d, 2)
                : v is long l
                    ? Math.Round(l / 1024d / 1024d / 1024d, 2)
                    : v is string s && ulong.TryParse(s, out var p)
                        ? Math.Round(p / 1024d / 1024d / 1024d, 2)
                        : 0d;
        }
        catch
        {
            return 0d;
        }
    }
}
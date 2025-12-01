//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   HardwareAnalyzer.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.Reflection;
using System.Runtime.CompilerServices;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;



// added
// added





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Hardware;


public sealed class HardwareAnalyzer : IAnalyzerModule
{


    private IActivityLogger? _logger;


    public string Name
    {
        get => "Hardware Analyzer";
    }


    public string Area
    {
        get => "Hardware";
    }





    public async Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger;
        var area = Area;
        _logger.Log("INF", "Start: Collecting hardware inventory via CIM", Ctx(area));
        List<string> warnings = [];
        List<string> errors = [];

        List<object> cpuList = [];
        List<object> memoryModules = [];
        List<object> disks = [];
        List<object> partitions = [];
        List<object> volumes = [];
        List<object> gpu = [];
        Dictionary<string, object?> baseboard = [];
        Dictionary<string, object?> enclosure = [];
        Dictionary<string, object?> tpm = [];
        List<object> battery = [];
        double totalMemGb = 0;

        try
        {
            _logger.Log("INF", "CPU: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> cpuRows = await context.Cim.QueryAsync(
                "SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, ProcessorId, SecondLevelAddressTranslationExtensions, VirtualizationFirmwareEnabled FROM Win32_Processor",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> mo in cpuRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            _logger.Log("INF", $"CPU: Complete count={cpuList.Count}", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"CPU query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"CPU: CPU query failed {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "Memory: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> compRows = await context.Cim.QueryAsync(
                "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> mo in compRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var bytes = mo.GetOrDefault("TotalPhysicalMemory");
                if (bytes is ulong ul) totalMemGb = Math.Round(ul / 1024d / 1024d / 1024d, 2);

                break;
            }

            IReadOnlyList<IDictionary<string, object?>> dimmRows = await context.Cim.QueryAsync(
                "SELECT BankLabel, Capacity, DeviceLocator, Manufacturer, PartNumber, SerialNumber, Speed FROM Win32_PhysicalMemory",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> dimm in dimmRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            _logger.Log("INF", $"Memory: Complete total={totalMemGb}GB modules={memoryModules.Count}", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Memory inventory failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Memory: Memory inventory failed {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "Disks: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> diskRows = await context.Cim.QueryAsync(
                "SELECT Index, Model, Size, MediaType, SerialNumber, InterfaceType FROM Win32_DiskDrive",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> mo in diskRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            IReadOnlyList<IDictionary<string, object?>> partRows = await context.Cim.QueryAsync(
                "SELECT DeviceID, DiskIndex, Index, Type, BootPartition, PrimaryPartition, Size FROM Win32_DiskPartition",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> p in partRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            IReadOnlyList<IDictionary<string, object?>> volRows = await context.Cim.QueryAsync(
                "SELECT DeviceID, VolumeName, FileSystem, Size, FreeSpace, DriveType FROM Win32_LogicalDisk WHERE DriveType=3",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> v in volRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            _logger.Log("INF", $"Disks: Complete disks={disks.Count} partitions={partitions.Count} volumes={volumes.Count}", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Disk inventory failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Disks: Disk inventory failed {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "GPU: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> gpuRows = await context.Cim.QueryAsync(
                "SELECT Name, AdapterCompatibility, DriverVersion, DriverDate, AdapterRAM, VideoProcessor FROM Win32_VideoController",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> vc in gpuRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            _logger.Log("INF", $"GPU: Complete count={gpu.Count}", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"GPU query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"GPU: GPU query failed {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "Board: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> bbRows = await context.Cim.QueryAsync(
                "SELECT Manufacturer, Product, SerialNumber, Version FROM Win32_BaseBoard",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> bb in bbRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                baseboard["Manufacturer"] = bb.GetOrDefault("Manufacturer");
                baseboard["Product"] = bb.GetOrDefault("Product");
                baseboard["SerialNumber"] = bb.GetOrDefault("SerialNumber");
                baseboard["Version"] = bb.GetOrDefault("Version");

                break;
            }

            IReadOnlyList<IDictionary<string, object?>> encRows = await context.Cim.QueryAsync(
                "SELECT ChassisTypes, Manufacturer, SMBIOSAssetTag FROM Win32_SystemEnclosure",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> enc in encRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                enclosure["ChassisTypes"] = enc.GetOrDefault("ChassisTypes");
                enclosure["Manufacturer"] = enc.GetOrDefault("Manufacturer");
                enclosure["AssetTag"] = enc.GetOrDefault("SMBIOSAssetTag");

                break;
            }

            _logger.Log("INF", "Board: Complete", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Baseboard/enclosure query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Board: Baseboard/enclosure query failed {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        var tpmPresent = false;
        try
        {
            _logger.Log("INF", "TPM: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> tRows = await context.Cim.QueryAsync(
                "SELECT IsEnabled_InitialValue, IsActivated_InitialValue, SpecVersion, ManufacturerIdTxt, ManufacturerVersion FROM Win32_Tpm",
                "\\\\.\\root\\CIMV2\\Security\\MicrosoftTpm", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> t in tRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                tpmPresent = true;
                tpm["IsEnabled_InitialValue"] = t.GetOrDefault("IsEnabled_InitialValue");
                tpm["IsActivated_InitialValue"] = t.GetOrDefault("IsActivated_InitialValue");
                tpm["SpecVersion"] = t.GetOrDefault("SpecVersion");
                tpm["ManufacturerIdTxt"] = t.GetOrDefault("ManufacturerIdTxt");
                tpm["ManufacturerVersion"] = t.GetOrDefault("ManufacturerVersion");

                break;
            }

            _logger.Log("INF", $"TPM: Complete present={tpmPresent}", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"TPM query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"TPM: TPM query failed {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "Battery: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> batRows = await context.Cim.QueryAsync(
                "SELECT Name, BatteryStatus, EstimatedChargeRemaining, EstimatedRunTime FROM Win32_Battery",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> b in batRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                battery.Add(new
                {
                    Name = b.GetOrDefault("Name"),
                    Status = b.GetOrDefault("BatteryStatus"),
                    ChargePercent = b.GetOrDefault("EstimatedChargeRemaining"),
                    EstimatedRunTimeMin = b.GetOrDefault("EstimatedRunTime")
                });
            }

            _logger.Log("INF", $"Battery: Complete count={battery.Count}", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Battery query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Battery: Battery query failed {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        var summary = new { CPUCount = cpuList.Count, MemoryGB = totalMemGb, DiskCount = disks.Count, GPUCount = gpu.Count, TpmPresent = tpmPresent };
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
        //  AreaResult result = new(area, summary, details, new List<Finding>().AsReadOnly(), warnings, errors);
        _logger.Log("INF", "Complete: Hardware inventory collected", Ctx(area));

        return null!;
    }





    private static double ToGb(object? v)
    {
        try
        {
            return v is ulong ul ? Math.Round(ul / 1024d / 1024d / 1024d, 2)
                : v is long l ? Math.Round(l / 1024d / 1024d / 1024d, 2)
                : v is string s && ulong.TryParse(s, out var p) ? Math.Round(p / 1024d / 1024d / 1024d, 2) : 0d;
        }
        catch
        {
            return 0d;
        }
    }





    private static string Ctx(string module, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
    {
        var asm = Assembly.GetExecutingAssembly().GetName().Name ?? "Analyzer";
        var f = Path.GetFileName(file);

        return $"{asm} - {module} - {member} - {f}";
    }


}
//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   OSAnalyzer.cs
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



// for assembly name
// for caller info





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.OS;


public sealed class OSAnalyzer : IAnalyzerModule
{


    private IActivityLogger? _logger;


    public string Name
    {
        get => "OS Analyzer";
    }


    public string Area
    {
        get => "OS";
    }





    public async Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger;
        var area = Area;
        _logger.Log("INF", "Start: Collecting OS and system information", Ctx(area));
        List<string> warnings = [];
        List<string> errors = [];

        Dictionary<string, object?> system = [];
        Dictionary<string, object?> os = [];
        Dictionary<string, object?> bios = [];
        Dictionary<string, object?> install = [];
        Dictionary<string, object?> pendingReboot = [];
        Dictionary<string, object?> services = [];
        List<object> servicesAutoIssues = [];
        List<object> updates = [];
        Dictionary<string, object?> timeInfo = [];
        List<object> pagefile = [];
        Dictionary<string, object?> power = [];
        Dictionary<string, object?> locale = [];

        try
        {
            _logger.Log("INF", "ComputerSystem: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> csRows = await context.Cim.QueryAsync(
                "SELECT Manufacturer, Model, Domain, PartOfDomain, TotalPhysicalMemory, NumberOfProcessors, NumberOfLogicalProcessors, SystemType FROM Win32_ComputerSystem",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> cs in csRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            _logger.Log("INF", "ComputerSystem: Complete", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"ComputerSystem query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"ComputerSystem: ComputerSystem query failed: {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "OperatingSystem: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> osRows = await context.Cim.QueryAsync(
                "SELECT Caption, Version, BuildNumber, CSDVersion, OSArchitecture, InstallDate, LastBootUpTime, SystemDirectory, WindowsDirectory, Locale, OSLanguage FROM Win32_OperatingSystem",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> o in osRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
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
                if (o.GetOrDefault("InstallDate") is string id && TryParseDmtfDate(id, out DateTimeOffset instUtc)) install["InstallDateUtc"] = instUtc;

                if (o.GetOrDefault("LastBootUpTime") is string lb && TryParseDmtfDate(lb, out DateTimeOffset bootUtc)) os["LastBootUpTimeUtc"] = bootUtc;

                break;
            }

            _logger.Log("INF", "OperatingSystem: Complete", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"OperatingSystem query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"OperatingSystem: OperatingSystem query failed: {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "BIOS: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> biosRows = await context.Cim.QueryAsync(
                "SELECT Manufacturer, SMBIOSBIOSVersion, SerialNumber, ReleaseDate FROM Win32_BIOS",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> b in biosRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                bios["Manufacturer"] = b.GetOrDefault("Manufacturer");
                bios["SMBIOSBIOSVersion"] = b.GetOrDefault("SMBIOSBIOSVersion");
                bios["SerialNumber"] = b.GetOrDefault("SerialNumber");
                bios["ReleaseDateRaw"] = b.GetOrDefault("ReleaseDate");

                break;
            }

            _logger.Log("INF", "BIOS: Complete", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"BIOS query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"BIOS: BIOS query failed: {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "PendingReboot: Start", Ctx(area));
            pendingReboot["CBS_RebootPending"] = KeyExists(context, "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Component Based Servicing", "RebootPending");
            pendingReboot["WU_RebootRequired"] = KeyExists(context, "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update", "RebootRequired");
            try
            {
                pendingReboot["PendingFileRenameOperations"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager", "PendingFileRenameOperations") is not null;
            }
            catch
            {
            }

            _logger.Log("INF", "PendingReboot: Complete", Ctx(area));
        }
        catch (Exception ex)
        {
            warnings.Add($"Pending reboot check failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"PendingReboot: Pending reboot check failed: {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "Services: Start", Ctx(area));
            int running = 0, stopped = 0, paused = 0;
            IReadOnlyList<IDictionary<string, object?>> svcRows = await context.Cim.QueryAsync(
                "SELECT Name, DisplayName, StartMode, State, PathName FROM Win32_Service",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> s in svcRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var state = s.GetOrDefault("State")?.ToString();
                var start = s.GetOrDefault("StartMode")?.ToString();
                if (string.Equals(state, "Running", StringComparison.OrdinalIgnoreCase))
                    running++;
                else if (string.Equals(state, "Stopped", StringComparison.OrdinalIgnoreCase))
                    stopped++;
                else if (string.Equals(state, "Paused", StringComparison.OrdinalIgnoreCase)) paused++;

                if (string.Equals(start, "Auto", StringComparison.OrdinalIgnoreCase) && !string.Equals(state, "Running", StringComparison.OrdinalIgnoreCase))
                    servicesAutoIssues.Add(new
                    {
                        Name = s.GetOrDefault("Name"),
                        DisplayName = s.GetOrDefault("DisplayName"),
                        StartMode = start,
                        State = state,
                        Path = s.GetOrDefault("PathName")
                    });
            }

            services["Running"] = running;
            services["Stopped"] = stopped;
            services["Paused"] = paused;
            services["AutoStartNotRunning"] = servicesAutoIssues;
            _logger.Log("INF", $"Services: Complete: running={running}, stopped={stopped}, paused={paused}, autoIssues={servicesAutoIssues.Count}", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Service enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Services: Service enumeration failed: {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "Updates: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> updRows = await context.Cim.QueryAsync(
                "SELECT HotFixID, Description, InstalledOn FROM Win32_QuickFixEngineering",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> q in updRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                updates.Add(new
                {
                    HotFixID = q.GetOrDefault("HotFixID"),
                    Description = q.GetOrDefault("Description"),
                    InstalledOn = q.GetOrDefault("InstalledOn")
                });
            }

            _logger.Log("INF", $"Updates: Complete: count={updates.Count}", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Updates (QFE) query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Updates: Updates (QFE) query failed: {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "Time: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> tzRows = await context.Cim.QueryAsync(
                "SELECT Bias, Caption, DaylightBias, DaylightName, StandardName FROM Win32_TimeZone",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> tz in tzRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                timeInfo["TimeZone"] = new
                {
                    tzCaption = tz.GetOrDefault("Caption"),
                    StandardName = tz.GetOrDefault("StandardName"),
                    DaylightName = tz.GetOrDefault("DaylightName")
                };

                break;
            }

            IReadOnlyList<IDictionary<string, object?>> w32Rows = await context.Cim.QueryAsync(
                "SELECT Name, State, StartMode FROM Win32_Service WHERE Name='W32Time'",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> s in w32Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                timeInfo["W32Time_State"] = s.GetOrDefault("State");
                timeInfo["W32Time_StartMode"] = s.GetOrDefault("StartMode");
            }

            try
            {
                timeInfo["NtpServer"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Services\\W32Time\\Parameters", "NtpServer");
            }
            catch
            {
            }

            try
            {
                timeInfo["Type"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Services\\W32Time\\Parameters", "Type");
            }
            catch
            {
            }

            _logger.Log("INF", "Time: Complete", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Time/W32Time query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Time: Time/W32Time query failed: {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "PageFile: Start", Ctx(area));
            IReadOnlyList<IDictionary<string, object?>> pfRows = await context.Cim.QueryAsync(
                "SELECT Name, AllocatedBaseSize, CurrentUsage, PeakUsage FROM Win32_PageFileUsage",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> pf in pfRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                pagefile.Add(new
                {
                    Name = pf.GetOrDefault("Name"),
                    AllocatedBaseSize = pf.GetOrDefault("AllocatedBaseSize"),
                    CurrentUsage = pf.GetOrDefault("CurrentUsage"),
                    PeakUsage = pf.GetOrDefault("PeakUsage")
                });
            }

            _logger.Log("INF", $"PageFile: Complete: entries={pagefile.Count}", Ctx(area));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"PageFile query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"PageFile: PageFile query failed: {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "Power: Start", Ctx(area));
            try
            {
                power["ActiveScheme"] = context.Registry.GetValue("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power\\User\\PowerSchemes", "ActivePowerScheme");
            }
            catch
            {
            }

            _logger.Log("INF", "Power: Complete", Ctx(area));
        }
        catch (Exception ex)
        {
            warnings.Add($"Power plan read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Power: Power plan read failed: {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

        try
        {
            _logger.Log("INF", "Locale: Start", Ctx(area));
            foreach (var name in new[] { "Locale", "LocaleName", "sShortDate", "sTimeFormat" })
            {
                try
                {
                    locale[name] = context.Registry.GetValue("HKCU\\Control Panel\\International", name) ?? string.Empty;
                }
                catch
                {
                }
            }

            _logger.Log("INF", "Locale: Complete", Ctx(area));
        }
        catch (Exception ex)
        {
            warnings.Add($"Locale read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Locale: Locale read failed: {ex.GetType().Name}: {ex.Message}", Ctx(area));
        }

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
        AreaResult result = new(area, summary, details, new List<Finding>().AsReadOnly(), warnings, errors);
        _logger.Log("INF", "Complete: OS and system information collected", Ctx(area));

        return result;
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
            DateTime dt = new(year, month, day, hour, minute, second, DateTimeKind.Local);
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
            {
                if (string.Equals(name, subKeyName, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }





    private static string Ctx(string module, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
    {
        var asm = Assembly.GetExecutingAssembly().GetName().Name ?? "Analyzer";
        var f = Path.GetFileName(file);

        return $"{asm} - {module} - {member} - {f}";
    }


}
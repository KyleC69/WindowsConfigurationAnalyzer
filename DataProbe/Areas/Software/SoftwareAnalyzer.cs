//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   SoftwareAnalyzer.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Software;


public sealed class SoftwareAnalyzer : IAnalyzerModule
{


    private IActivityLogger? _logger;


    public string Name => "Software Analyzer";
    public string Area => "Software";





    public async Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        string area = Area;
        _logger.Log("INF", "Start: Collecting software inventory", area);
        List<string> warnings = [];
        List<string> errors = [];

        List<object> installed = [];
        try
        {
            _logger.Log("INF", "Installed: Start", area);
            foreach (string? relPath in new[] { "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall" })
            {
                ReadUninstallKey(context, installed, $"HKLM\\{relPath}");
                ReadUninstallKey(context, installed, $"HKCU\\{relPath}");
            }

            _logger.Log("INF", $"Installed: Complete: count={installed.Count}", area);
        }
        catch (Exception ex)
        {
            warnings.Add($"Registry enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Installed: Registry enumeration failed ({ex.Message})", area);
        }

        List<object> processes = [];
        try
        {
            _logger.Log("INF", "Processes: Start", area);
            foreach (Process p in Process.GetProcesses())
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Capture PID first and skip special system PIDs.
                    int pid;
                    try
                    {
                        pid = p.Id;
                    }
                    catch
                    {
                        continue;
                    }

                    if (pid is 0 or 4)
                    {
                        continue; // skip Idle/System
                    }

                    string name = string.Empty;
                    string? path = null;

                    // These property accesses can fail for protected processes; ignore per-item failures.
                    try
                    {
                        name = p.ProcessName;
                    }
                    catch
                    {
                        /* ignore */
                    }

                    try
                    {
                        path = Native.TryGetImagePath(pid);
                    }
                    catch
                    {
                        /* ignore */
                    }

                    processes.Add(new { Name = name, Path = path, Id = pid });
                }
                catch
                {
                    // Narrow scope: ignore any per-process failure and continue.
                }
                finally
                {
                    try
                    {
                        p.Dispose();
                    }
                    catch
                    {
                        /* best effort */
                    }
                }
            }

            _logger.Log("INF", $"Processes: Complete: count={processes.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Process enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Processes: Process enumeration failed ({ex.Message})", area);
        }

        List<object> optionalFeatures = [];
        try
        {
            _logger.Log("INF", "OptionalFeatures: Start", area);
            IReadOnlyList<IDictionary<string, object?>> optRows = await context.Cim.QueryAsync(
                "SELECT Name, InstallState, Caption FROM Win32_OptionalFeature", null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> f in optRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                optionalFeatures.Add(new { Name = f.GetOrDefault("Name"), State = f.GetOrDefault("InstallState"), Caption = f.GetOrDefault("Caption") });
            }

            _logger.Log("INF", $"OptionalFeatures: Complete: count={optionalFeatures.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Optional features query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"OptionalFeatures: Optional features query failed ({ex.Message})", area);
        }

        List<object> serverFeatures = [];
        try
        {
            _logger.Log("INF", "ServerFeatures: Start", area);
            IReadOnlyList<IDictionary<string, object?>> sfRows = await context.Cim.QueryAsync(
                "SELECT ID, Name, InstallState FROM Win32_ServerFeature", null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> sf in sfRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                serverFeatures.Add(new { Id = sf.GetOrDefault("ID"), Name = sf.GetOrDefault("Name"), State = sf.GetOrDefault("InstallState") });
            }

            _logger.Log("INF", $"ServerFeatures: Complete: count={serverFeatures.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Server features query failed or not supported: {ex.Message}");
            _logger.Log("WRN", $"ServerFeatures: Server features query failed or not supported ({ex.Message})", area);
        }

        List<object> storeApps = [];
        try
        {
            _logger.Log("INF", "StoreApps: Start", area);
            IReadOnlyList<IDictionary<string, object?>> storeRows = await context.Cim.QueryAsync(
                "SELECT Name, Version, InstallLocation, Publisher FROM Win32_InstalledStoreProgram", null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> a in storeRows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                storeApps.Add(new { Name = a.GetOrDefault("Name"), Version = a.GetOrDefault("Version"), InstallLocation = a.GetOrDefault("InstallLocation"), Publisher = a.GetOrDefault("Publisher") });
            }

            _logger.Log("INF", $"StoreApps: Complete: count={storeApps.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Store apps query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"StoreApps: Store apps query failed ({ex.Message})", area);
        }

        List<object> provisionedAppx = [];
        try
        {
            _logger.Log("INF", "ProvisionedAppx: Start", area);
            string root = "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Appx\\AppxAllUserStore\\Applications";
            foreach (string app in context.Registry.EnumerateSubKeys(root))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string baseKey = $"{root}\\{app}";
                string? fullName = context.Registry.GetValue(baseKey, "PackageFullName")?.ToString();
                string? moniker = context.Registry.GetValue(baseKey, "PackageMoniker")?.ToString();
                string? idName = context.Registry.GetValue(baseKey, "PackageIdName")?.ToString();
                provisionedAppx.Add(new { Key = app, PackageFullName = fullName, PackageMoniker = moniker, PackageIdName = idName });
            }

            _logger.Log("INF", $"ProvisionedAppx: Complete: count={provisionedAppx.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Provisioned Appx read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"ProvisionedAppx: Provisioned Appx read failed ({ex.Message})", area);
        }

        List<object> provisioningPkgs = [];
        try
        {
            _logger.Log("INF", "ProvisioningPkgs: Start", area);
            foreach (string? dir in new[]
                     {
                         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Recovery", "Customizations"),
                         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Provisioning", "Packages")
                     })
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Directory.Exists(dir))
                {
                    foreach (string ppkg in Directory.EnumerateFiles(dir, "*.ppkg", SearchOption.AllDirectories))
                    {
                        FileInfo fi = new(ppkg);
                        provisioningPkgs.Add(new { Path = ppkg, Size = fi.Length, LastWriteUtc = fi.LastWriteTimeUtc });
                    }
                }
            }

            _logger.Log("INF", $"ProvisioningPkgs: Complete: count={provisioningPkgs.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Provisioning packages scan failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"ProvisioningPkgs: Provisioning packages scan failed ({ex.Message})", area);
        }

        var summary = new { InstalledCount = installed.Count, RunningProcesses = processes.Count, OptionalFeatures = optionalFeatures.Count, StoreApps = storeApps.Count };
        var details = new
        {
            Installed = installed,
            RunningProcesses = processes,
            OptionalFeatures = optionalFeatures,
            ServerFeatures = serverFeatures,
            StoreApps = storeApps,
            ProvisionedAppx = provisionedAppx,
            ProvisioningPackages = provisioningPkgs
        };
        AreaResult result = new(area, summary, details, new List<Finding>().AsReadOnly(), warnings, errors);
        _logger.Log("INF", "Complete: Software inventory collected", area);

        return result;
    }





    private static void ReadUninstallKey(IAnalyzerContext context, List<object> target, string hiveAndPath)
    {
        foreach (string sub in context.Registry.EnumerateSubKeys(hiveAndPath))
        {
            string basePath = $"{hiveAndPath}\\{sub}";
            string? name = context.Registry.GetValue(basePath, "DisplayName")?.ToString();

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            string? ver = context.Registry.GetValue(basePath, "DisplayVersion")?.ToString();
            string? pub = context.Registry.GetValue(basePath, "Publisher")?.ToString();
            string? installDate = context.Registry.GetValue(basePath, "InstallDate")?.ToString();
            target.Add(new { Name = name, Version = ver, Publisher = pub, InstallDate = installDate, Key = basePath });
        }
    }


}



// Add once
internal static class Native
{


    private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;





    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageName(IntPtr hProcess, int flags, StringBuilder text, ref int size);





    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint access, bool inherit, int processId);





    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);





    public static string? TryGetImagePath(int pid)
    {
        nint h = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);

        if (h == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            StringBuilder sb = new(1024);
            int len = sb.Capacity;

            return QueryFullProcessImageName(h, 0, sb, ref len) ? sb.ToString(0, len) : null;
        }
        finally
        {
            CloseHandle(h);
        }
    }


}
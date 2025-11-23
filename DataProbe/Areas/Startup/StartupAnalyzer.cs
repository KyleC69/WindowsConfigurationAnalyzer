//  Created:  2025/10/30
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   StartupAnalyzer.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Startup;


public sealed class StartupAnalyzer : IAnalyzerModule
{


    private IActivityLogger? _logger;


    public string Name => "Startup/Autoruns Analyzer";
    public string Area => "Startup";





    public async Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        string area = Area;
        _logger.Log("INF", "Start: Collecting startup and persistence entries", area);
        List<string> warnings = [];
        List<string> errors = [];

        List<object> runEntries = [];
        List<object> approvedEntries = [];
        List<object> startupFolderEntries = [];
        List<object> scheduledTasks = [];
        List<object> servicesAutoStart = [];
        Dictionary<string, object?> winlogon = [];
        Dictionary<string, object?> appInitDlls = [];
        List<object> browserHelperObjects = [];
        List<object> shellExecuteHooks = [];
        List<object> shellServiceObjects = [];
        List<object> shellExtensionsApproved = [];
        List<object> lsaPackages = [];
        List<object> policyRun = [];
        List<object> ifeoDebuggers = [];
        List<object> ifeoOthers = [];
        List<object> wmiSubscriptions = [];
        List<object> wmiConsumers = [];
        List<object> activeSetup = [];

        try
        {
            _logger.Log("INF", "RunKeys: Start", area);
            foreach (string? root in new[] { "HKLM", "HKCU" })
            {
                foreach (string? sub in new[]
                         {
                             "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                             "Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce",
                             "Software\\Microsoft\\Windows\\CurrentVersion\\RunOnceEx",
                             "Software\\Microsoft\\Windows\\CurrentVersion\\RunServices",
                             "Software\\Microsoft\\Windows\\CurrentVersion\\RunServicesOnce",
                             "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run",
                             "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce"
                         })
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string basePath = $"{root}\\{sub}";
                    foreach (string name in context.Registry.EnumerateValueNames(basePath))
                    {
                        string? val = context.Registry.GetValue(basePath, name)?.ToString();
                        runEntries.Add(new { HivePath = basePath, Name = name, Command = val });
                    }
                }
            }

            _logger.Log("INF", $"RunKeys: Complete: count={runEntries.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Startup registry enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"RunKeys: Startup registry enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "StartupApproved: Start", area);
            foreach (string? root in new[] { "HKLM", "HKCU" })
            {
                foreach (string? sub in new[]
                         {
                             "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run",
                             "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run32",
                             "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\StartupFolder"
                         })
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string basePath = $"{root}\\{sub}";
                    foreach (string name in context.Registry.EnumerateValueNames(basePath))
                    {
                        object? v = context.Registry.GetValue(basePath, name);
                        approvedEntries.Add(new
                        {
                            HivePath = basePath,
                            Name = name,
                            State = v is byte[] b ? BitConverter.ToString(b) : v
                        });
                    }
                }
            }

            _logger.Log("INF", $"StartupApproved: Complete: count={approvedEntries.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"StartupApproved enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"StartupApproved: StartupApproved enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "PolicyRun: Start", area);
            foreach (string? root in new[] { "HKLM", "HKCU" })
            {
                cancellationToken.ThrowIfCancellationRequested();
                string basePath = $"{root}\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\\Run";
                foreach (string name in context.Registry.EnumerateValueNames(basePath))
                {
                    string? val = context.Registry.GetValue(basePath, name)?.ToString();
                    policyRun.Add(new { HivePath = basePath, Name = name, Command = val });
                }
            }

            _logger.Log("INF", $"PolicyRun: Complete: count={policyRun.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Policies\\Explorer\\Run enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"PolicyRun: Policies\\Explorer\\Run enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "StartupFolders: Start", area);
            foreach (Environment.SpecialFolder sp in new[] { Environment.SpecialFolder.Startup, Environment.SpecialFolder.CommonStartup })
            {
                cancellationToken.ThrowIfCancellationRequested();
                string path = Environment.GetFolderPath(sp);
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
                    {
                        startupFolderEntries.Add(new { Folder = path, File = Path.GetFileName(file) });
                    }
                }
            }

            _logger.Log("INF", $"StartupFolders: Complete: count={startupFolderEntries.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Startup folder enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"StartupFolders: Startup folder enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "ScheduledTasks: Start", area);
            string tasksRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "Tasks");
            if (Directory.Exists(tasksRoot))
            {
                foreach (string file in Directory.EnumerateFiles(tasksRoot, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string rel = file.Substring(tasksRoot.Length).TrimStart(Path.DirectorySeparatorChar);
                    FileInfo info = new(file);
                    scheduledTasks.Add(new { Task = rel, Size = info.Length, LastWriteUtc = info.LastWriteTimeUtc });
                }
            }

            _logger.Log("INF", $"ScheduledTasks: Complete: count={scheduledTasks.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Scheduled tasks enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"ScheduledTasks: Scheduled tasks enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "Services: Start", area);
            string servicesKey = "HKLM\\SYSTEM\\CurrentControlSet\\Services";
            foreach (string svc in context.Registry.EnumerateSubKeys(servicesKey))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string basePath = $"{servicesKey}\\{svc}";
                object? start = context.Registry.GetValue(basePath, "Start");
                if (start is int i and 2)
                {
                    string? image = context.Registry.GetValue(basePath, "ImagePath")?.ToString();
                    object? delayed = context.Registry.GetValue(basePath, "DelayedAutoStart");
                    string? objName = context.Registry.GetValue(basePath, "ObjectName")?.ToString();
                    string? desc = context.Registry.GetValue(basePath, "Description")?.ToString();
                    string? svcDll = context.Registry.GetValue(basePath + "\\Parameters", "ServiceDll")?.ToString();
                    servicesAutoStart.Add(new { Name = svc, Start = i, ImagePath = image, DelayedAutoStart = delayed, ObjectName = objName, Description = desc, ServiceDll = svcDll });
                }
            }

            _logger.Log("INF", $"Services: Complete: count={servicesAutoStart.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Services enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Services: Services enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "Winlogon: Start", area);
            string key = "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon";
            foreach (string? name in new[] { "Shell", "Userinit" })
            {
                string? v = context.Registry.GetValue(key, name)?.ToString();
                if (v is not null)
                {
                    winlogon[name] = v;
                }
            }

            foreach (string sub in context.Registry.EnumerateSubKeys($"{key}\\Notify"))
            {
                winlogon[$"Notify:{sub}"] = context.Registry.GetValue($"{key}\\Notify\\{sub}", "DLLName")?.ToString();
            }

            _logger.Log("INF", "Winlogon: Complete", area);
        }
        catch (Exception ex)
        {
            warnings.Add($"Winlogon enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Winlogon: Winlogon enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "AppInit: Start", area);
            string key = "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Windows";
            foreach (string? name in new[] { "AppInit_DLLs", "LoadAppInit_DLLs" })
            {
                object? v = context.Registry.GetValue(key, name);
                if (v is not null)
                {
                    appInitDlls[name] = v;
                }
            }

            _logger.Log("INF", "AppInit: Complete", area);
        }
        catch (Exception ex)
        {
            warnings.Add($"AppInit_DLLs read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"AppInit: AppInit_DLLs read failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "BHO: Start", area);
            foreach (string? path in new[]
                     {
                         "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects",
                         "HKLM\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects",
                         "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects"
                     })
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (string clsid in context.Registry.EnumerateSubKeys(path))
                {
                    string? dll = null;
                    try
                    {
                        dll = context.Registry.GetValue($"HKCR\\CLSID\\{clsid}\\InprocServer32", "")?.ToString();
                    }
                    catch
                    {
                    }

                    browserHelperObjects.Add(new { Path = path, Clsid = clsid, Dll = dll });
                }
            }

            _logger.Log("INF", $"BHO: Complete: count={browserHelperObjects.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"BHO enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"BHO: BHO enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "ShellExecuteHooks: Start", area);
            foreach (string? path in new[]
                     {
                         "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ShellExecuteHooks",
                         "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ShellExecuteHooks"
                     })
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (string name in context.Registry.EnumerateValueNames(path))
                {
                    string? v = context.Registry.GetValue(path, name)?.ToString();
                    shellExecuteHooks.Add(new { Path = path, Name = name, Value = v });
                }
            }

            _logger.Log("INF", $"ShellExecuteHooks: Complete: count={shellExecuteHooks.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"ShellExecuteHooks enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"ShellExecuteHooks: ShellExecuteHooks enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "ShellServiceObjectDelayLoad: Start", area);
            foreach (string? path in new[]
                     {
                         "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ShellServiceObjectDelayLoad",
                         "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ShellServiceObjectDelayLoad"
                     })
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (string name in context.Registry.EnumerateValueNames(path))
                {
                    string? v = context.Registry.GetValue(path, name)?.ToString();
                    shellServiceObjects.Add(new { Path = path, Name = name, Value = v });
                }
            }

            _logger.Log("INF", $"ShellServiceObjectDelayLoad: Complete: count={shellServiceObjects.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"ShellServiceObjectDelayLoad enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"ShellServiceObjectDelayLoad: ShellServiceObjectDelayLoad enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "ShellExtensionsApproved: Start", area);
            foreach (string? path in new[]
                     {
                         "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved",
                         "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved"
                     })
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (string name in context.Registry.EnumerateValueNames(path))
                {
                    string? v = context.Registry.GetValue(path, name)?.ToString();
                    shellExtensionsApproved.Add(new { Path = path, Name = name, Value = v });
                }
            }

            _logger.Log("INF", $"ShellExtensionsApproved: Complete: count={shellExtensionsApproved.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Shell Extensions Approved enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"ShellExtensionsApproved: Shell Extensions Approved enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "LSA: Start", area);
            string path = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa";
            foreach (string? name in new[] { "Authentication Packages", "Notification Packages" })
            {
                object? v = context.Registry.GetValue(path, name);
                if (v is string s)
                {
                    lsaPackages.Add(new { Name = name, Value = s });
                }
                else if (v is string[] arr)
                {
                    foreach (string item in arr)
                    {
                        lsaPackages.Add(new { Name = name, Value = item });
                    }
                }
            }

            _logger.Log("INF", $"LSA: Complete: count={lsaPackages.Count}", area);
        }
        catch (Exception ex)
        {
            warnings.Add($"LSA packages read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"LSA: LSA packages read failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "IFEO: Start", area);
            string baseKey = "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options";
            foreach (string exe in context.Registry.EnumerateSubKeys(baseKey))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string key = $"{baseKey}\\{exe}";
                string? dbg = context.Registry.GetValue(key, "Debugger")?.ToString();
                string? gflag = context.Registry.GetValue(key, "GlobalFlag")?.ToString();
                string? useFilter = context.Registry.GetValue(key, "UseFilter")?.ToString();
                if (!string.IsNullOrWhiteSpace(dbg))
                {
                    ifeoDebuggers.Add(new { Executable = exe, Debugger = dbg });
                }

                if (!string.IsNullOrEmpty(gflag) || !string.IsNullOrEmpty(useFilter))
                {
                    ifeoOthers.Add(new { Executable = exe, GlobalFlag = gflag, UseFilter = useFilter });
                }
            }

            _logger.Log("INF", $"IFEO: Complete: debuggers={ifeoDebuggers.Count}, other={ifeoOthers.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"IFEO enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"IFEO: IFEO enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "WMI: Start", area);
            IReadOnlyList<IDictionary<string, object?>> filters = await context.Cim.QueryAsync(
                "SELECT Name, Query, EventNamespace FROM __EventFilter", "\\\\.\\root\\subscription", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> f in filters)
            {
                cancellationToken.ThrowIfCancellationRequested();
                wmiSubscriptions.Add(new { Type = "Filter", Name = f.GetOrDefault("Name")?.ToString(), Query = f.GetOrDefault("Query")?.ToString(), Namespace = f.GetOrDefault("EventNamespace")?.ToString() });
            }

            IReadOnlyList<IDictionary<string, object?>> binds = await context.Cim.QueryAsync("SELECT * FROM __FilterToConsumerBinding", "\\\\.\\root\\subscription", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> b in binds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                wmiSubscriptions.Add(new { Type = "Binding", Filter = b.GetOrDefault("Filter")?.ToString(), Consumer = b.GetOrDefault("Consumer")?.ToString() });
            }

            foreach ((string? query, string? type, string[]? props) in new (string q, string type, string[] props)[]
                     {
                         ("SELECT Name, CommandLineTemplate FROM CommandLineEventConsumer", "CommandLineEventConsumer", ["Name", "CommandLineTemplate"]),
                         ("SELECT Name, ScriptText FROM ActiveScriptEventConsumer", "ActiveScriptEventConsumer", ["Name", "ScriptText"]),
                         ("SELECT Name, Category FROM NTEventLogEventConsumer", "NTEventLogEventConsumer", ["Name", "Category"]),
                         ("SELECT Name, DeliveryAddress FROM SMTPEventConsumer", "SMTPEventConsumer", ["Name", "DeliveryAddress"]),
                         ("SELECT Name, Filename FROM LogFileEventConsumer", "LogFileEventConsumer", ["Name", "Filename"])
                     })
            {
                IReadOnlyList<IDictionary<string, object?>> rows = await context.Cim.QueryAsync(query, "\\\\.\\root\\subscription", cancellationToken).ConfigureAwait(false);
                foreach (IDictionary<string, object?> c in rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Dictionary<string, object?> dict = new() { ["Type"] = type };
                    foreach (string p in props)
                    {
                        dict[p] = c.GetOrDefault(p);
                    }

                    wmiConsumers.Add(dict);
                }
            }

            _logger.Log("INF", $"WMI: Complete: filters+bindings={wmiSubscriptions.Count}, consumers={wmiConsumers.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"WMI subscription/consumer enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"WMI: WMI subscription/consumer enumeration failed ({ex.Message})", area);
        }

        try
        {
            _logger.Log("INF", "ActiveSetup: Start", area);
            foreach (string? root in new[] { "HKLM", "HKCU" })
            {
                cancellationToken.ThrowIfCancellationRequested();
                string baseKey = $"{root}\\SOFTWARE\\Microsoft\\Active Setup\\Installed Components";
                foreach (string sub in context.Registry.EnumerateSubKeys(baseKey))
                {
                    string k = $"{baseKey}\\{sub}";
                    string? disp = context.Registry.GetValue(k, "DisplayName")?.ToString();
                    string? ver = context.Registry.GetValue(k, "Version")?.ToString();
                    string? stub = context.Registry.GetValue(k, "StubPath")?.ToString();
                    activeSetup.Add(new { HivePath = k, DisplayName = disp, Version = ver, StubPath = stub });
                }
            }

            _logger.Log("INF", $"ActiveSetup: Complete: count={activeSetup.Count}", area);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Active Setup enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"ActiveSetup: Active Setup enumeration failed ({ex.Message})", area);
        }

        var summary = new
        {
            Run = runEntries.Count,
            StartupApproved = approvedEntries.Count,
            StartupFolder = startupFolderEntries.Count,
            ScheduledTasks = scheduledTasks.Count,
            AutoStartServices = servicesAutoStart.Count,
            BHOs = browserHelperObjects.Count,
            ShellExecuteHooks = shellExecuteHooks.Count,
            ShellServiceObjects = shellServiceObjects.Count,
            ShellExtensionsApproved = shellExtensionsApproved.Count,
            IFEO = ifeoDebuggers.Count + ifeoOthers.Count,
            WmiSubscriptions = wmiSubscriptions.Count,
            WmiConsumers = wmiConsumers.Count,
            ActiveSetup = activeSetup.Count
        };
        var details = new
        {
            Run = runEntries,
            StartupApproved = approvedEntries,
            StartupFolder = startupFolderEntries,
            ScheduledTasks = scheduledTasks,
            AutoStartServices = servicesAutoStart,
            Winlogon = winlogon,
            AppInit = appInitDlls,
            BHOs = browserHelperObjects,
            ShellExecuteHooks = shellExecuteHooks,
            ShellServiceObjects = shellServiceObjects,
            ShellExtensionsApproved = shellExtensionsApproved,
            LsaPackages = lsaPackages,
            PolicyRun = policyRun,
            IFEO_Debuggers = ifeoDebuggers,
            IFEO_Other = ifeoOthers,
            WmiSubscriptions = wmiSubscriptions,
            WmiConsumers = wmiConsumers,
            ActiveSetup = activeSetup
        };

        AreaResult result = new(area, summary, details, new List<Finding>().AsReadOnly(), warnings, errors);
        _logger.Log("INF", "Complete: Startup and persistence entries collected", area);

        return result;
    }


}
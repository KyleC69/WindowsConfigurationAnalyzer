// Created:  2025/10/30
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  StartupAnalyzer.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Startup;



public sealed class StartupAnalyzer : IAnalyzerModule
{
    public string Name => "Startup/Autoruns Analyzer";
    public string Area => "Startup";





    public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
    {
        string area = Area;
        context.ActionLogger.Info(area, "Start", "Collecting startup and persistence entries");
        List<string> warnings = new();
        List<string> errors = new();

        List<object> runEntries = new();
        List<object> approvedEntries = new();
        List<object> startupFolderEntries = new();
        List<object> scheduledTasks = new();
        List<object> servicesAutoStart = new();
        Dictionary<string, object?> winlogon = new();
        Dictionary<string, object?> appInitDlls = new();
        List<object> browserHelperObjects = new();
        List<object> shellExecuteHooks = new();
        List<object> shellServiceObjects = new();
        List<object> shellExtensionsApproved = new();
        List<object> lsaPackages = new();
        List<object> policyRun = new();
        List<object> ifeoDebuggers = new();
        List<object> ifeoOthers = new();
        List<object> wmiSubscriptions = new();
        List<object> wmiConsumers = new();
        List<object> activeSetup = new();

        // Classic Run/RunOnce across HKLM/HKCU (including Wow6432Node and RunOnceEx)
        try
        {
            context.ActionLogger.Info(area, "RunKeys", "Start");
            foreach (string root in new[] { "HKLM", "HKCU" })
            {
                foreach (string sub in new[]
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
                    string basePath = $"{root}\\{sub}";
                    foreach (string name in context.Registry.EnumerateValueNames(basePath))
                    {
                        string? val = context.Registry.GetValue(basePath, name)?.ToString();
                        runEntries.Add(new { HivePath = basePath, Name = name, Command = val });
                    }
                }
            }

            context.ActionLogger.Info(area, "RunKeys", $"Complete: count={runEntries.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Startup registry enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "RunKeys", "Startup registry enumeration failed", ex);
        }

        // StartupApproved (Explorer)
        try
        {
            context.ActionLogger.Info(area, "StartupApproved", "Start");
            foreach (string root in new[] { "HKLM", "HKCU" })
            {
                foreach (string sub in new[]
                         {
                             "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run",
                             "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run32",
                             "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\StartupFolder"
                         })
                {
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

            context.ActionLogger.Info(area, "StartupApproved", $"Complete: count={approvedEntries.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"StartupApproved enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "StartupApproved", "StartupApproved enumeration failed", ex);
        }

        // Group Policy Run
        try
        {
            context.ActionLogger.Info(area, "PolicyRun", "Start");
            foreach (string root in new[] { "HKLM", "HKCU" })
            {
                string basePath = $"{root}\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\\Run";
                foreach (string name in context.Registry.EnumerateValueNames(basePath))
                {
                    string? val = context.Registry.GetValue(basePath, name)?.ToString();
                    policyRun.Add(new { HivePath = basePath, Name = name, Command = val });
                }
            }

            context.ActionLogger.Info(area, "PolicyRun", $"Complete: count={policyRun.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Policies\\Explorer\\Run enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "PolicyRun", "Policies\\Explorer\\Run enumeration failed", ex);
        }

        // Startup folders
        try
        {
            context.ActionLogger.Info(area, "StartupFolders", "Start");
            foreach (var sp in new[]
                         { Environment.SpecialFolder.Startup, Environment.SpecialFolder.CommonStartup })
            {
                string path = Environment.GetFolderPath(sp);
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
                    {
                        startupFolderEntries.Add(new { Folder = path, File = Path.GetFileName(file) });
                    }
                }
            }

            context.ActionLogger.Info(area, "StartupFolders", $"Complete: count={startupFolderEntries.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Startup folder enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "StartupFolders", "Startup folder enumeration failed", ex);
        }

        // Scheduled tasks (names only)
        try
        {
            context.ActionLogger.Info(area, "ScheduledTasks", "Start");
            string tasksRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32",
                "Tasks");
            if (Directory.Exists(tasksRoot))
            {
                foreach (string file in Directory.EnumerateFiles(tasksRoot, "*", SearchOption.AllDirectories))
                {
                    string rel = file.Substring(tasksRoot.Length).TrimStart(Path.DirectorySeparatorChar);
                    FileInfo info = new(file);
                    scheduledTasks.Add(new { Task = rel, Size = info.Length, LastWriteUtc = info.LastWriteTimeUtc });
                }
            }

            context.ActionLogger.Info(area, "ScheduledTasks", $"Complete: count={scheduledTasks.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Scheduled tasks enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "ScheduledTasks", "Scheduled tasks enumeration failed", ex);
        }

        // Auto-start services (Start=2, include properties)
        try
        {
            context.ActionLogger.Info(area, "Services", "Start");
            string servicesKey = "HKLM\\SYSTEM\\CurrentControlSet\\Services";
            foreach (string svc in context.Registry.EnumerateSubKeys(servicesKey))
            {
                string basePath = $"{servicesKey}\\{svc}";
                object? start = context.Registry.GetValue(basePath, "Start");
                if (start is int i && i == 2)
                {
                    string? image = context.Registry.GetValue(basePath, "ImagePath")?.ToString();
                    object? delayed = context.Registry.GetValue(basePath, "DelayedAutoStart");
                    string? objName = context.Registry.GetValue(basePath, "ObjectName")?.ToString();
                    string? desc = context.Registry.GetValue(basePath, "Description")?.ToString();
                    string? svcDll = context.Registry.GetValue(basePath + "\\Parameters", "ServiceDll")?.ToString();
                    servicesAutoStart.Add(new
                    {
                        Name = svc,
                        Start = i,
                        ImagePath = image,
                        DelayedAutoStart = delayed,
                        ObjectName = objName,
                        Description = desc,
                        ServiceDll = svcDll
                    });
                }
            }

            context.ActionLogger.Info(area, "Services", $"Complete: count={servicesAutoStart.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Services enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Services", "Services enumeration failed", ex);
        }

        // Winlogon entries
        try
        {
            context.ActionLogger.Info(area, "Winlogon", "Start");
            string key = "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon";
            foreach (string name in new[] { "Shell", "Userinit" })
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

            context.ActionLogger.Info(area, "Winlogon", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"Winlogon enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Winlogon", "Winlogon enumeration failed", ex);
        }

        // AppInit_DLLs
        try
        {
            context.ActionLogger.Info(area, "AppInit", "Start");
            string key = "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Windows";
            foreach (string name in new[] { "AppInit_DLLs", "LoadAppInit_DLLs" })
            {
                object? v = context.Registry.GetValue(key, name);
                if (v is not null)
                {
                    appInitDlls[name] = v;
                }
            }

            context.ActionLogger.Info(area, "AppInit", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"AppInit_DLLs read failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "AppInit", "AppInit_DLLs read failed", ex);
        }

        // Browser Helper Objects + resolve CLSID InprocServer32
        try
        {
            context.ActionLogger.Info(area, "BHO", "Start");
            foreach (string path in new[]
                     {
                         "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects",
                         "HKLM\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects",
                         "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects"
                     })
            {
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

            context.ActionLogger.Info(area, "BHO", $"Complete: count={browserHelperObjects.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"BHO enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "BHO", "BHO enumeration failed", ex);
        }

        // ShellExecuteHooks
        try
        {
            context.ActionLogger.Info(area, "ShellExecuteHooks", "Start");
            foreach (string path in new[]
                     {
                         "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ShellExecuteHooks",
                         "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ShellExecuteHooks"
                     })
            {
                foreach (string name in context.Registry.EnumerateValueNames(path))
                {
                    string? v = context.Registry.GetValue(path, name)?.ToString();
                    shellExecuteHooks.Add(new { Path = path, Name = name, Value = v });
                }
            }

            context.ActionLogger.Info(area, "ShellExecuteHooks", $"Complete: count={shellExecuteHooks.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"ShellExecuteHooks enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "ShellExecuteHooks", "ShellExecuteHooks enumeration failed", ex);
        }

        // ShellServiceObjectDelayLoad
        try
        {
            context.ActionLogger.Info(area, "ShellServiceObjectDelayLoad", "Start");
            foreach (string path in new[]
                     {
                         "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ShellServiceObjectDelayLoad",
                         "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ShellServiceObjectDelayLoad"
                     })
            {
                foreach (string name in context.Registry.EnumerateValueNames(path))
                {
                    string? v = context.Registry.GetValue(path, name)?.ToString();
                    shellServiceObjects.Add(new { Path = path, Name = name, Value = v });
                }
            }

            context.ActionLogger.Info(area, "ShellServiceObjectDelayLoad",
                $"Complete: count={shellServiceObjects.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"ShellServiceObjectDelayLoad enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "ShellServiceObjectDelayLoad",
                "ShellServiceObjectDelayLoad enumeration failed", ex);
        }

        // Shell Extensions Approved
        try
        {
            context.ActionLogger.Info(area, "ShellExtensionsApproved", "Start");
            foreach (string path in new[]
                     {
                         "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved",
                         "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved"
                     })
            {
                foreach (string name in context.Registry.EnumerateValueNames(path))
                {
                    string? v = context.Registry.GetValue(path, name)?.ToString();
                    shellExtensionsApproved.Add(new { Path = path, Name = name, Value = v });
                }
            }

            context.ActionLogger.Info(area, "ShellExtensionsApproved",
                $"Complete: count={shellExtensionsApproved.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Shell Extensions Approved enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "ShellExtensionsApproved", "Shell Extensions Approved enumeration failed",
                ex);
        }

        // LSA Packages
        try
        {
            context.ActionLogger.Info(area, "LSA", "Start");
            string path = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Lsa";
            foreach (string name in new[] { "Authentication Packages", "Notification Packages" })
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

            context.ActionLogger.Info(area, "LSA", $"Complete: count={lsaPackages.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"LSA packages read failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "LSA", "LSA packages read failed", ex);
        }

        // IFEO Debugger and other interesting values
        try
        {
            context.ActionLogger.Info(area, "IFEO", "Start");
            string baseKey = "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options";
            foreach (string exe in context.Registry.EnumerateSubKeys(baseKey))
            {
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

            context.ActionLogger.Info(area, "IFEO",
                $"Complete: debuggers={ifeoDebuggers.Count}, other={ifeoOthers.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"IFEO enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "IFEO", "IFEO enumeration failed", ex);
        }

        // WMI persistence (root\\subscription)
        try
        {
            context.ActionLogger.Info(area, "WMI", "Start");
            foreach (var f in context.Cim.Query(
                         "SELECT Name, Query, EventNamespace FROM __EventFilter",
                         "\\\\.\\root\\subscription"))
            {
                string? name = f.GetOrDefault("Name")?.ToString();
                string? query = f.GetOrDefault("Query")?.ToString();
                string? evns = f.GetOrDefault("EventNamespace")?.ToString();
                wmiSubscriptions.Add(new { Type = "Filter", Name = name, Query = query, Namespace = evns });
            }

            foreach (var b in context.Cim.Query("SELECT * FROM __FilterToConsumerBinding",
                         "\\\\.\\root\\subscription"))
            {
                string? flt = b.GetOrDefault("Filter")?.ToString();
                string? cons = b.GetOrDefault("Consumer")?.ToString();
                wmiSubscriptions.Add(new { Type = "Binding", Filter = flt, Consumer = cons });
            }

            foreach (var c in context.Cim.Query(
                         "SELECT Name, CommandLineTemplate FROM CommandLineEventConsumer",
                         "\\\\.\\root\\subscription"))
            {
                wmiConsumers.Add(new
                {
                    Type = "CommandLineEventConsumer",
                    Name = c.GetOrDefault("Name"),
                    Command = c.GetOrDefault("CommandLineTemplate")
                });
            }

            foreach (var c in context.Cim.Query(
                         "SELECT Name, ScriptText FROM ActiveScriptEventConsumer",
                         "\\\\.\\root\\subscription"))
            {
                wmiConsumers.Add(new
                {
                    Type = "ActiveScriptEventConsumer",
                    Name = c.GetOrDefault("Name"),
                    Script = c.GetOrDefault("ScriptText")
                });
            }

            foreach (var c in context.Cim.Query(
                         "SELECT Name, Category FROM NTEventLogEventConsumer",
                         "\\\\.\\root\\subscription"))
            {
                wmiConsumers.Add(new
                {
                    Type = "NTEventLogEventConsumer",
                    Name = c.GetOrDefault("Name"),
                    Category = c.GetOrDefault("Category")
                });
            }

            foreach (var c in context.Cim.Query(
                         "SELECT Name, DeliveryAddress FROM SMTPEventConsumer",
                         "\\\\.\\root\\subscription"))
            {
                wmiConsumers.Add(new
                {
                    Type = "SMTPEventConsumer",
                    Name = c.GetOrDefault("Name"),
                    Address = c.GetOrDefault("DeliveryAddress")
                });
            }

            foreach (var c in context.Cim.Query(
                         "SELECT Name, Filename FROM LogFileEventConsumer",
                         "\\\\.\\root\\subscription"))
            {
                wmiConsumers.Add(new
                {
                    Type = "LogFileEventConsumer",
                    Name = c.GetOrDefault("Name"),
                    File = c.GetOrDefault("Filename")
                });
            }

            context.ActionLogger.Info(area, "WMI",
                $"Complete: filters+bindings={wmiSubscriptions.Count}, consumers={wmiConsumers.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"WMI subscription/consumer enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "WMI", "WMI subscription/consumer enumeration failed", ex);
        }

        // Active Setup Installed Components
        try
        {
            context.ActionLogger.Info(area, "ActiveSetup", "Start");
            foreach (string root in new[] { "HKLM", "HKCU" })
            {
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

            context.ActionLogger.Info(area, "ActiveSetup", $"Complete: count={activeSetup.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Active Setup enumeration failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "ActiveSetup", "Active Setup enumeration failed", ex);
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

        AreaResult result = new(area, summary, details, Array.Empty<Finding>(), warnings, errors);
        context.ActionLogger.Info(area, "Complete", "Startup and persistence entries collected");

        return Task.FromResult(result);
    }
}
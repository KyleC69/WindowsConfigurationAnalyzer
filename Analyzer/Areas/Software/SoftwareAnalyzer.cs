using WindowsConfigurationAnalyzer.Contracts;
using WindowsConfigurationAnalyzer.Models;

namespace WindowsConfigurationAnalyzer.Software;

public sealed class SoftwareAnalyzer : IAnalyzerModule
{
 public string Name => "Software Analyzer";
 public string Area => "Software";

 public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
 {
 var area = Area;
 context.ActionLogger.Info(area, "Collect", "Collecting installed software and running processes");
 var warnings = new List<string>();
 var errors = new List<string>();

 var installed = new List<object>();
 try
 {
 foreach (var relPath in new[]
 {
 "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall",
 "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall"
 })
 {
 ReadUninstallKey(context, installed, $"HKLM\\{relPath}");
 ReadUninstallKey(context, installed, $"HKCU\\{relPath}");
 }
 }
 catch (Exception ex)
 {
 warnings.Add($"Registry enumeration failed: {ex.Message}");
 }

 var processes = new List<object>();
 try
 {
 foreach (var p in System.Diagnostics.Process.GetProcesses())
 {
 string name = string.Empty;
 string? path = null;
 try
 {
 name = p.ProcessName;
 try { path = p.MainModule?.FileName; } catch { /* access denied */ }
 }
 catch { }
 processes.Add(new { Name = name, Path = path, Id = p.Id });
 }
 }
 catch (Exception ex)
 {
 warnings.Add($"Process enumeration failed: {ex.Message}");
 }

 var summary = new { InstalledCount = installed.Count, RunningProcesses = processes.Count };
 var details = new { Installed = installed, RunningProcesses = processes };
 var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), warnings, errors);
 context.ActionLogger.Info(area, "Collect", "Software information collected");
 return Task.FromResult(result);
 }

 private static void ReadUninstallKey(IAnalyzerContext context, List<object> target, string hiveAndPath)
 {
 foreach (var sub in context.Registry.EnumerateSubKeys(hiveAndPath))
 {
 var basePath = $"{hiveAndPath}\\{sub}";
 var name = context.Registry.GetValue(basePath, "DisplayName")?.ToString();
 if (string.IsNullOrWhiteSpace(name)) continue; // skip non-display entries
 var ver = context.Registry.GetValue(basePath, "DisplayVersion")?.ToString();
 var pub = context.Registry.GetValue(basePath, "Publisher")?.ToString();
 var installDate = context.Registry.GetValue(basePath, "InstallDate")?.ToString();
 target.Add(new { Name = name, Version = ver, Publisher = pub, InstallDate = installDate, Key = basePath });
 }
 }
}

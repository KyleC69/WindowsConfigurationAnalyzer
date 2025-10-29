using WindowsConfigurationAnalyzer.Contracts;
using WindowsConfigurationAnalyzer.Models;
using WindowsConfigurationAnalyzer.Utilities;

namespace WindowsConfigurationAnalyzer.Security;

public sealed class SecurityAnalyzer : IAnalyzerModule
{
 public string Name => "Security Analyzer";
 public string Area => "Security";

 public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
 {
 var area = Area;
 context.ActionLogger.Info(area, "Collect", "Collecting security configuration");
 var warnings = new List<string>();
 var errors = new List<string>();

 // Windows Security Center - anti-virus info (when available)
 var avInfo = new List<object>();
 try
 {
 // SecurityCenter2 for Windows10/11
 foreach (var mo in context.Cim.Query("SELECT * FROM AntiVirusProduct", "\\\\.\\root\\SecurityCenter2"))
 {
 avInfo.Add(new
 {
 Name = mo.GetOrDefault("displayName"),
 Path = mo.GetOrDefault("pathToSignedProductExe"),
 State = mo.GetOrDefault("productState"),
 Timestamp = mo.GetOrDefault("timestamp")
 });
 }
 }
 catch (Exception ex)
 {
 warnings.Add($"Security Center query failed: {ex.Message}");
 }

 // Basic policy hints (read-only)
 object? uac = null;
 try
 {
 uac = context.Registry.GetValue("HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "EnableLUA");
 }
 catch (Exception ex)
 {
 warnings.Add($"Registry read failed: {ex.Message}");
 }

 var summary = new { AntivirusProducts = avInfo.Count, UacEnabled = (uac is int i && i!=0) };
 var details = new { Antivirus = avInfo, UAC = uac };
 var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), warnings, errors);
 context.ActionLogger.Info(area, "Collect", "Security configuration collected");
 return Task.FromResult(result);
 }
}

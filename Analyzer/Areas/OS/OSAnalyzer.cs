using WindowsConfigurationAnalyzer.Contracts;
using WindowsConfigurationAnalyzer.Models;

namespace WindowsConfigurationAnalyzer.OS;

public sealed class OSAnalyzer : IAnalyzerModule
{
 public string Name => "OS Analyzer";
 public string Area => "OS";

 public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
 {
 var area = Area;
 context.ActionLogger.Info(area, "Collect", "Collecting basic OS information");

 var info = new
 {
 MachineName = Environment.MachineName,
 OSVersion = Environment.OSVersion.VersionString,
 Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
 UserDomainName = Environment.UserDomainName,
 UserName = Environment.UserName
 };

 var summary = new { Name = info.OSVersion, Is64Bit = info.Is64BitOperatingSystem };
 var details = info;

 var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), Array.Empty<string>(), Array.Empty<string>());
 context.ActionLogger.Info(area, "Collect", "OS information collected");
 return Task.FromResult(result);
 }
}

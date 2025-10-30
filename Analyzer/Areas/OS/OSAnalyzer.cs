using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.OS;

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
            MachineName = context.Environment.MachineName,
            OSVersion = context.Environment.OSVersionString,
            Is64BitOperatingSystem = context.Environment.Is64BitOS,
            UserDomainName = context.Environment.UserDomainName,
            UserName = context.Environment.UserName
        };

        var summary = new { Name = info.OSVersion, Is64Bit = info.Is64BitOperatingSystem };
        var details = info;

        var result = new AreaResult(area, summary, details, Array.Empty<Finding>(), Array.Empty<string>(), Array.Empty<string>());
        context.ActionLogger.Info(area, "Collect", "OS information collected");
        return Task.FromResult(result);
    }
}

// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  DemoRun.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Diagnostics;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Engine;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Export;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;



namespace KC.WindowsConfigurationAnalyzer.Analyzer;



public static class DemoRun
{
    public static async Task RunOnceAsync(IServiceProvider services, CancellationToken ct = default(CancellationToken))
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("WCA.Demo");
        AnalyzerEngine engine = new(logger);
        var ctx = services.GetRequiredService<IAnalyzerContext>();
        foreach (var m in services.GetServices<IAnalyzerModule>())
        {
            engine.AddModule(m);
        }

        var result = await engine.RunAllAsync(ctx, ct);

        // Evaluate rules (temporary, few starters)
        RuleEngine ruleEngine = new(new IRule[]
        {
            new AvMissingRule(),
            new DuplicateDnsRule()
        });
        var extraFindings = ruleEngine.Evaluate(result);
        var merged = result with { GlobalFindings = result.GlobalFindings.Concat(extraFindings).ToList() };

        string stamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        string computer = merged.ComputerName;
        string outputDir = Path.Combine("exports", computer, DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"));
        string jsonPath = Path.Combine(outputDir, $"WCA-{computer}-{stamp}Z.json");
        string htmlPath = Path.Combine(outputDir, $"WCA-{computer}-{stamp}Z.html");
        await new JsonExporter().ExportAsync(merged, jsonPath, ct);
        await new HtmlReportBuilder().ExportAsync(merged, htmlPath, ct);
        logger.LogInformation("Exported JSON: {Json} and HTML: {Html}", jsonPath, htmlPath);
    }
}
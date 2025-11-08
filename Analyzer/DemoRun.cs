// Created:  2025/10/29
// Solution:
// Project:
// File:
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
	public static async Task RunOnceAsync(IServiceProvider services, CancellationToken ct = default)
	{
		var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("WCA.Demo");
		var engine = new AnalyzerEngine(logger);
		var ctx = services.GetRequiredService<IAnalyzerContext>();
		foreach (var m in services.GetServices<IAnalyzerModule>()) engine.AddModule(m);

		var result = await engine.RunAllAsync(ctx, ct);

		// Evaluate rules (temporary, few starters)
		var ruleEngine = new RuleEngine(new IRule[]
		{
			new AvMissingRule(),
			new DuplicateDnsRule()
		});
		var extraFindings = ruleEngine.Evaluate(result);
		var merged = result with { GlobalFindings = result.GlobalFindings.Concat(extraFindings).ToList() };

		var stamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
		var computer = merged.ComputerName;
		var outputDir = Path.Combine("exports", computer, DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"));
		var jsonPath = Path.Combine(outputDir, $"WCA-{computer}-{stamp}Z.json");
		var htmlPath = Path.Combine(outputDir, $"WCA-{computer}-{stamp}Z.html");
		await new JsonExporter().ExportAsync(merged, jsonPath, ct);
		await new HtmlReportBuilder().ExportAsync(merged, htmlPath, ct);
		logger.LogInformation("Exported JSON: {Json} and HTML: {Html}", jsonPath, htmlPath);
	}
}
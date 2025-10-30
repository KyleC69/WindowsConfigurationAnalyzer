using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Engine;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Context;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Export;


namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

public partial class AnalyzerViewModel : ObservableRecipient
{
    private readonly IServiceProvider _services;
    private readonly ILocalSettingsService _localSettings;

    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private string statusMessage = "Idle";

    public ObservableCollection<string> StatusMessages { get; } = new();

    public AnalyzerViewModel(IServiceProvider services)
    {
        _services = services;
        _localSettings = App.GetService<ILocalSettingsService>();
    }

    private void Log(string message)
    {
        StatusMessage = message;
        StatusMessages.Add($"[{DateTimeOffset.Now:HH:mm:ss}] {message}");
    }

    private static string ApplyTemplate(string template)
    {
        var now = DateTimeOffset.UtcNow;
        return template
            .Replace("{MachineName}", Environment.MachineName)
            .Replace("{yyyy-MM-dd}", now.ToString("yyyy-MM-dd"))
            .Replace("{yyyyMMdd-HHmm}", now.ToString("yyyyMMdd-HHmm"))
            .Replace("{HHmm}", now.ToString("HHmm"));
    }

    [RelayCommand]
    private async Task RunAnalyzerAsync()
    {
        if (IsRunning)
        {
            return;
        }

        try
        {
            IsRunning = true;
            Log("Starting analyzer...");

            using var scope = _services.CreateScope();
            var sp = scope.ServiceProvider;
            var loggerFactory = sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var engine = new AnalyzerEngine(loggerFactory.CreateLogger("WCA.UI.Run"));
            foreach (var m in sp.GetServices<IAnalyzerModule>())
            {
                engine.AddModule(m);
            }
            var ctx = sp.GetRequiredService<IAnalyzerContext>();

            // Ensure activity log file per run and event log redundancy
            var logTemplate = await _localSettings.ReadSettingAsync<string>("LogPathTemplate") ?? "logs/{yyyyMMdd-HHmm}.txt";
            var logPath = ApplyTemplate(logTemplate);
            var logDir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            var fileSink = new FileActionLogSink(logPath);
            var eventLogSink = new EventLogSink();
            var appLogger = loggerFactory.CreateLogger("WCA.ActionLogger");
            var actionLogger = new ActionLogger(appLogger, fileSink, eventLogSink);
            var contextWithFile = new AnalyzerContext(
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("WCA"),
                sp.GetRequiredService<ITimeProvider>(),
                actionLogger,
                sp.GetRequiredService<IRegistryReader>(),
                sp.GetRequiredService<ICimReader>(),
                sp.GetRequiredService<IEventLogReader>(),
                sp.GetRequiredService<IFirewallReader>(),
                sp.GetRequiredService<IEnvReader>()
            );

            Log($"Logging to: {logPath}");
            Log("Running analyzers...");
            var result = await engine.RunAllAsync(contextWithFile);

            // Export per area, per run using template
            var exportTemplate = await _localSettings.ReadSettingAsync<string>("ExportPathTemplate")
                 ?? ($"exports/{Environment.MachineName}/{{yyyy-MM-dd}}/{{HHmm}}.json");
            foreach (var area in result.Areas)
            {
                var path = ApplyTemplate(exportTemplate);
                // place per-area file name
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var file = Path.Combine(dir!, $"{area.Area}.json");
                await new JsonExporter().ExportAsync(
                    new AnalyzerResult(result.ComputerName, result.ExportTimestampUtc, new List<AreaResult> { area }, result.GlobalFindings, result.ActionLog),
                    file,
                    CancellationToken.None);
            }
            Log($"Completed. Exports at template root: {Path.GetDirectoryName(ApplyTemplate(exportTemplate))}");
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand]
    private void OpenExportsFolder()
    {
        try
        {
            var exportRoot = Path.Combine(AppContext.BaseDirectory, "exports");
            if (!Directory.Exists(exportRoot))
            {
                Directory.CreateDirectory(exportRoot);
            }
            Process.Start(new ProcessStartInfo
            {
                FileName = exportRoot,
                UseShellExecute = true
            });
            Log($"Opened exports folder: {exportRoot}");
        }
        catch (Exception ex)
        {
            Log($"Open folder failed: {ex.Message}");
        }
    }
}

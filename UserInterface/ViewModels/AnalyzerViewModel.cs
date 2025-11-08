// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Collections.ObjectModel;
using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Context;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Diagnostics;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Engine;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Export;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;



public partial class AnalyzerViewModel : ObservableRecipient
{
    private readonly ILocalSettingsService _localSettings;
    private readonly IServiceProvider _services;

    // AOT/WinRT-safe properties using explicit backing fields
    private bool _isRunning;

    private string _statusMessage = "Idle";

    public IAsyncRelayCommand RunAnalyzerCommand
    {
        get;
    }
    public IRelayCommand OpenFolderCommand
    {
        get;
    }





    public AnalyzerViewModel()
        : this(App.GetService<IServiceProvider>())
    {

    }

    public AnalyzerViewModel(IServiceProvider services)
    {
        _services = services;
        _localSettings = App.GetService<ILocalSettingsService>();
        RunAnalyzerCommand = new AsyncRelayCommand(RunAnalyzerAsync);
        OpenFolderCommand = new RelayCommand(OpenExportsFolder);
    }





    public bool IsRunning
    {
        get => _isRunning;
        set => SetProperty(ref _isRunning, value);
    }



    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }



    public ObservableCollection<string> StatusMessages { get; } = [];





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
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var eventProvider = sp.GetService<IEventProvider>();
            var engine = new AnalyzerEngine(loggerFactory.CreateLogger("WCA.UI.Run"), eventProvider: eventProvider);
            foreach (var m in sp.GetServices<IAnalyzerModule>()) engine.AddModule(m);
            var ctx = sp.GetRequiredService<IAnalyzerContext>();

            // Ensure activity log file per run
            var logTemplate = await _localSettings.ReadSettingAsync<string>("LogPathTemplate") ??
                              "logs/{yyyyMMdd-HHmm}.txt";
            var logPath = ApplyTemplate(logTemplate);
            var logDir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            var fileSink = new FileActionLogSink(logPath);
            var appLogger = loggerFactory.CreateLogger("WCA.ActionLogger");
            var actionLogger = new ActionLogger(appLogger, fileSink, eventProvider);
            var contextWithFile = new AnalyzerContext(
                sp.GetRequiredService<ILoggerFactory>().CreateLogger("WCA"),
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

            // Evaluate comprehensive rules
            var rules = new IRule[]
            {
                new AvMissingRule(),
                new DuplicateDnsRule(),
                new FirewallDisabledRule(),
                new HighCpuRule(),
                new LowMemoryRule(),
                new SuspiciousAutorunsRule(),
                new RdpExposedRule()
            };
            var ruleEngine = new RuleEngine(rules);
            var extraFindings = ruleEngine.Evaluate(result);
            var merged = result with { GlobalFindings = result.GlobalFindings.Concat(extraFindings).ToList() };

            // Export per area, per run using template
            var exportTemplate = await _localSettings.ReadSettingAsync<string>("ExportPathTemplate")
                                 ?? $"exports/{Environment.MachineName}/{{yyyy-MM-dd}}/{{HHmm}}.json";
            foreach (var area in merged.Areas)
            {
                var path = ApplyTemplate(exportTemplate);
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var file = Path.Combine(dir!, $"{area.Area}.json");
                await new JsonExporter().ExportAsync(
                    new AnalyzerResult(merged.ComputerName, merged.ExportTimestampUtc, [area], merged.GlobalFindings,
                        merged.ActionLog),
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
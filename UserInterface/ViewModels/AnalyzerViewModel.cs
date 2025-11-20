// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  AnalyzerViewModel.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Versioning;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Engine;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Export;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Etw;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

using Microsoft.Extensions.DependencyInjection;




namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

// Helper model for UI selection of analyzer modules
public sealed partial class AnalyzerModuleSelection : ObservableObject
{
    public AnalyzerModuleSelection(IAnalyzerModule module)
    {
        Module = module;
        Name = module.Name;
        Area = module.Area;
    }

    public IAnalyzerModule Module { get; }
    public string Name { get; }
    public string Area { get; }

    // Pseudocode:
    // - Replace [ObservableProperty] field with explicit backing field '_isSelected' default true.
    // - Implement public property 'IsSelected' using SetProperty to notify changes.
    private bool _isSelected = true; // default selected
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}


public partial class AnalyzerViewModel : ObservableRecipient
{


    private readonly ILocalSettingsService _localSettings;
    private readonly IServiceProvider _services;
    private readonly Guid _sessionId = Guid.NewGuid(); //Application runtime session id ??

    private readonly CancellationTokenSource cts = new();

    // AOT/WinRT-safe properties using explicit backing fields
    private bool _isRunning;

    private string _statusMessage = "Idle";
    private readonly IActivityLogger _activityLogger;





    public AnalyzerViewModel() : this(App.GetService<IServiceProvider>())
    {
    }





    public AnalyzerViewModel(IServiceProvider services)
    {
        _services = services;
        _localSettings = services.GetRequiredService<ILocalSettingsService>();
        _activityLogger = services.GetRequiredService<IActivityLogger>();
        RunAnalyzerCommand = new AsyncRelayCommand(RunAnalyzerAsync);
        OpenFolderCommand = new RelayCommand(OpenExportsFolder);
        SelectAllCommand = new RelayCommand(SelectAllModules);

        // Populate module selections from DI
        Modules = new ObservableCollection<AnalyzerModuleSelection>(
            _services.GetServices<IAnalyzerModule>()
                     .Select(m => new AnalyzerModuleSelection(m))
        );
    }





    public IAsyncRelayCommand RunAnalyzerCommand { get; }

    public IRelayCommand OpenFolderCommand { get; }

    public IRelayCommand SelectAllCommand { get; }

    public ObservableCollection<AnalyzerModuleSelection> Modules { get; }


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
        //  StatusMessages.Add($"[{DateTimeOffset.Now:HH:mm:ss}] {message}");
        ActivityLogger.Log("INF", message, "AnalyzerViewModel");
    }





    private static string ApplyTemplate(string template)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return template
            .Replace("{MachineName}", Environment.MachineName)
            .Replace("{yyyy-MM-dd}", now.ToString("yyyy-MM-dd"))
            .Replace("{yyyyMMdd-HHmm}", now.ToString("yyyyMMdd-HHmm"))
            .Replace("{HHmm}", now.ToString("HHmm"));
    }





    private static string GetExportDirectory(string appliedTemplate)
    {
        // If template resolves to a file path (has extension), use its directory; otherwise treat as directory
        return Path.HasExtension(appliedTemplate)
            ? (Path.GetDirectoryName(appliedTemplate) ?? appliedTemplate)
            : appliedTemplate;
    }

    private void SelectAllModules()
    {
        foreach (var m in Modules)
        {
            m.IsSelected = true;
        }
    }





    private async Task RunAnalyzerAsync()
    {

        ActivityLogger.Log("INF", "Starting analyzer...", "RunAnalyzerAsync");

        if (IsRunning)
        {
            return;
        }

        var selected = Modules.Where(m => m.IsSelected).Select(m => m.Module).ToList();
        if (selected.Count == 0)
        {
            Log("No analyzers selected.");
            return;
        }

        var correlationIdString = Guid.NewGuid().ToString("N"); // Unique correlation ID per run
        IsRunning = true;
        try
        {
            // Run only selected modules
            AnalyzerRunner runner = new(selected);
            AnalyzerResult result = await runner.RunAllAsync(correlationIdString, _activityLogger, cts.Token);

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

            RuleEngine ruleEngine = new(rules);
            IReadOnlyList<Finding> extraFindings = ruleEngine.Evaluate(result);
            AnalyzerResult merged = result with { GlobalFindings = result.GlobalFindings.Concat(extraFindings).ToList() };

            // Export per area, per run using template
            var exportTemplate = await _localSettings.ReadApplicationSettingAsync<string>("ExportPathTemplate")
                                 ?? Path.Combine(
                                     Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                     "WindowsConfigurationAnalyzer",
                                     "exports",
                                     "{MachineName}",
                                     "{yyyy-MM-dd}",
                                     "{HHmm}.json");

            var applied = ApplyTemplate(exportTemplate);
            var exportDir = GetExportDirectory(applied);

            var exportCount = 0;
            WCAEventSource.Log.SessionStart(SessionId: Guid.NewGuid().ToString("N"), Environment.MachineName, "1.0.0", correlationIdString);
            foreach (AreaResult area in merged.Areas)
            {
                try
                {
                    if (!string.IsNullOrEmpty(exportDir))
                    {
                        Directory.CreateDirectory(exportDir);
                    }

                    var file = Path.Combine(exportDir, $"{area.Area}.json");
                    await new JsonExporter().ExportAsync(
                        new AnalyzerResult(merged.ComputerName, merged.ExportTimestampUtc, [area], merged.GlobalFindings),
                        file,
                        CancellationToken.None);
                    exportCount++;
                }
                catch (Exception e)
                {
                    WCAEventSource.Log.ExceptionError(SessionId: _sessionId.ToString("N"), CorrelationId: correlationIdString, e.Message, e.StackTrace?.ToString(), Context: "Exporting area results");

                }
            }

            try
            {
                // Write a consolidated HTML report to help verify report generation
                var htmlPath = Path.Combine(exportDir, "Report.html");
                await new HtmlReportBuilder().ExportAsync(merged, htmlPath, CancellationToken.None);
            }
            catch (Exception e)
            {
                WCAEventSource.Log.ExceptionError(SessionId: _sessionId.ToString("N"), CorrelationId: correlationIdString, ExceptMessage: e.Message, ExceptStack: e.StackTrace?.ToString(), Context: "Exporting HTML report");
            }

            if (exportCount == 0)
            {
                Log("No area results were exported. Check analyzers and export path template.");
            }

            Log($"Completed. Exports at: {exportDir}");
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
            WCAEventSource.Log.ExceptionError(SessionId: _sessionId.ToString("N"), CorrelationId: correlationIdString, ExceptMessage: ex.Message, ExceptStack: ex.StackTrace?.ToString(), Context: "Running analyzer");
        }
        finally
        {
            IsRunning = false;
            //WCAEventSource.Log.SessionStop(SessionId: _sessionId.ToString("N"), Areas: merged.Areas, CorrelationId: correlationIdString); TODO: Fix params to pass necessary data
        }
    }





    private void OpenExportsFolder()
    {
        try
        {
            var exportRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "WindowsConfigurationAnalyzer",
                "exports");
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
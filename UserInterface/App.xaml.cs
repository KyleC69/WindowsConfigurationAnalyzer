// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  App.xaml.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.DependencyInjection;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Engine;
using KC.WindowsConfigurationAnalyzer.UserInterface.Activation;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Etw;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;
using KC.WindowsConfigurationAnalyzer.UserInterface.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using KC.WindowsConfigurationAnalyzer.UserInterface.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using WinUIEx;

using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;





namespace KC.WindowsConfigurationAnalyzer.UserInterface;


// To learn more about WinUI3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{


    public static PerformanceCounter? LogCounter;

    public static string? ProjectDir => Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "ProjectDirectory")?.Value;



    public App()
    {
        InitializeComponent();


        //<SaveManifestToFile(out var message);

        WCAEventSource.Log.SessionStart(Guid.NewGuid().ToString(), Environment.MachineName, "1.09.0.0", Guid.NewGuid().ToString());

        /*
        if (!string.IsNullOrEmpty(message))
        {
            WCAEventSource.Log.ActionFailed("Manifest registration has failed.", message);
            ActivityLogger.Log("ERR", $"Manifest registration has failed. {message}", "App.xaml.cs");
        }
       */
        WCAEventSource.Log.ActionStart("App Initialization has started.");
        // Initialize logging counters - Must be done before ActivityLogger.Initialize
        // SetupCounters();


        ActivityLogger.Initialize(true);

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {

                // Default Activation Handler
                ActivityLogger.Log("INF", "Loading Default Activation Handler", "App.xaml.cs");
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();
                ActivityLogger.Log("INF", "Default Activation Handler Loaded", "App.xaml.cs");

                // Other Activation Handlers
                ActivityLogger.Log("INF", "Loading App Notification Activation Handler", "App.xaml.cs");
                services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();
                ActivityLogger.Log("INF", "App Notification Activation Handler Loaded", "App.xaml.cs");

                // Services
                ActivityLogger.Log("INF", "Loading IAppNotificationService", "App.xaml.cs");
                services.AddSingleton<IAppNotificationService, AppNotificationService>();
                ActivityLogger.Log("INF", "IAppNotificationService Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading ILocalSettingsService", "App.xaml.cs");
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                ActivityLogger.Log("INF", "ILocalSettingsService Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading IThemeSelectorService", "App.xaml.cs");
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                ActivityLogger.Log("INF", "IThemeSelectorService Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading INavigationViewService", "App.xaml.cs");
                services.AddTransient<INavigationViewService, NavigationViewService>();
                ActivityLogger.Log("INF", "INavigationViewService Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading IActivationService", "App.xaml.cs");
                services.AddSingleton<IActivationService, ActivationService>();
                ActivityLogger.Log("INF", "IActivationService Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading IPageService", "App.xaml.cs");
                services.AddSingleton<IPageService, PageService>();
                ActivityLogger.Log("INF", "IPageService Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading INavigationService", "App.xaml.cs");
                services.AddSingleton<INavigationService, NavigationService>();
                ActivityLogger.Log("INF", "INavigationService Loaded", "App.xaml.cs");

                // Core Services
                ActivityLogger.Log("INF", "Loading ISampleDataService", "App.xaml.cs");
                services.AddSingleton<ISampleDataService, SampleDataService>();
                ActivityLogger.Log("INF", "ISampleDataService Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading IFileService", "App.xaml.cs");
                services.AddSingleton<IFileService, FileService>();
                ActivityLogger.Log("INF", "IFileService Loaded", "App.xaml.cs");


                ActivityLogger.Log("INF", "Analyzer Core Services Loading", "App.xaml.cs");
                services.AddWcaCore();
                ActivityLogger.Log("INF", "Analyzer Services Loaded", "App.xaml.cs");

                services.AddSingleton<IActivityLogger, ActivityLogAdapter>();

                services.AddTransient<AnalyzerRunner>();


                // Views and ViewModels
                ActivityLogger.Log("INF", "Loading EventingViewModel", "App.xaml.cs");
                services.AddTransient<EventingViewModel>();
                ActivityLogger.Log("INF", "EventingViewModel Loaded", "App.xaml.cs");
                ActivityLogger.Log("INF", "Loading EventingPage", "App.xaml.cs");
                services.AddTransient<EventingPage>();
                ActivityLogger.Log("INF", "EventingPage Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading AnalyzerViewModel", "App.xaml.cs");
                services.AddTransient<AnalyzerViewModel>();
                ActivityLogger.Log("INF", "AnalyzerViewModel Loaded", "App.xaml.cs");
                ActivityLogger.Log("INF", "Loading AnalyzerPage", "App.xaml.cs");
                services.AddTransient<AnalyzerPage>();
                ActivityLogger.Log("INF", "AnalyzerPage Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading SettingsViewModel", "App.xaml.cs");
                services.AddTransient<SettingsViewModel>();
                ActivityLogger.Log("INF", "SettingsViewModel Loaded", "App.xaml.cs");
                ActivityLogger.Log("INF", "Loading SettingsPage", "App.xaml.cs");
                services.AddTransient<SettingsPage>();
                ActivityLogger.Log("INF", "SettingsPage Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading ApplicationsViewModel", "App.xaml.cs");
                services.AddTransient<ApplicationsViewModel>();
                ActivityLogger.Log("INF", "ApplicationsViewModel Loaded", "App.xaml.cs");
                ActivityLogger.Log("INF", "Loading ApplicationsPage", "App.xaml.cs");
                services.AddTransient<ApplicationsPage>();
                ActivityLogger.Log("INF", "ApplicationsPage Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading WmiRegistryViewModel", "App.xaml.cs");
                services.AddTransient<WmiRegistryViewModel>();
                ActivityLogger.Log("INF", "WmiRegistryViewModel Loaded", "App.xaml.cs");
                ActivityLogger.Log("INF", "Loading WmiRegistryPage", "App.xaml.cs");
                services.AddTransient<WmiRegistryPage>();
                ActivityLogger.Log("INF", "WmiRegistryPage Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading DriversViewModel", "App.xaml.cs");
                services.AddTransient<DriversViewModel>();
                ActivityLogger.Log("INF", "DriversViewModel Loaded", "App.xaml.cs");
                ActivityLogger.Log("INF", "Loading DriversPage", "App.xaml.cs");
                services.AddTransient<DriversPage>();
                ActivityLogger.Log("INF", "DriversPage Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading ServicesViewModel", "App.xaml.cs");
                services.AddTransient<ServicesViewModel>();
                ActivityLogger.Log("INF", "ServicesViewModel Loaded", "App.xaml.cs");
                ActivityLogger.Log("INF", "Loading ServicesPage", "App.xaml.cs");
                services.AddTransient<ServicesPage>();
                ActivityLogger.Log("INF", "ServicesPage Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading ReportViewModel", "App.xaml.cs");
                services.AddTransient<ReportViewModel>();
                ActivityLogger.Log("INF", "ReportViewModel Loaded", "App.xaml.cs");
                ActivityLogger.Log("INF", "Loading ReportPage", "App.xaml.cs");
                services.AddTransient<ReportPage>();
                ActivityLogger.Log("INF", "ReportPage Loaded", "App.xaml.cs");

                ActivityLogger.Log("INF", "Loading ShellPage", "App.xaml.cs");
                services.AddTransient<ShellPage>();
                ActivityLogger.Log("INF", "ShellPage Loaded", "App.xaml.cs");
                ActivityLogger.Log("INF", "Loading ShellViewModel", "App.xaml.cs");
                services.AddTransient<ShellViewModel>();
                ActivityLogger.Log("INF", "ShellViewModel Loaded", "App.xaml.cs");
            }).Build();

        GetService<IAppNotificationService>().Initialize();

        Current.UnhandledException += App_UnhandledException;


        ActivityLogger.Log("INF", "Services built, exiting Application .ctor", "App.xaml.cs");
        WCAEventSource.Log.ActionStop("App Initialization has completed.");
    }





    private void SaveManifestToFile(out string? message)
    {
        message = null!;

        var manifest =  EventSource.GenerateManifest(typeof(WCAEventSource),"WCA-ProviderResources.dll", EventManifestOptions.Strict);
        var manpath = Path.Combine(ProjectDir!, "WCA-Provider-Ops.man");
        File.WriteAllText(manpath, manifest);
        message = $"Manifest saved to {manpath}";

    }




    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost? Host { get; set; }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public static Application AppHost => Current;




    public static T GetService<T>() where T : class
    {

        return (Current as App)!.Host!.Services.GetService(typeof(T)) is not T service
            ? throw new ArgumentException(
                $"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.")
            : service;
    }





    private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        ActivityLogger.Log("ERR", e.Message, "UnhandledException");
        EventLog.WriteEntry("Windows Configuration Analyzer",
            $"Unhandled Exception: {e.Message}\n{e.Exception.StackTrace}", EventLogEntryType.Error);

        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }





    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(),
                AppContext.BaseDirectory));

            await GetService<IActivationService>().ActivateAsync(args);


            base.OnLaunched(args);
        }
        catch (Exception e)
        {
            ActivityLogger.Log("ERR", e.Message, "OnLaunched:Failed");
            EventLog.WriteEntry("Windows Configuration Analyzer",
                $"Fatal Exception during OnLaunched: {e.Message}\n{e.StackTrace}", EventLogEntryType.Error);
        }
    }





    private static bool SetupCounters()
    {
        if (!PerformanceCounterCategory.Exists("LoggingCountersCategory"))
        {
            var counterDataCollection = new CounterCreationDataCollection
            {
                new CounterCreationData("LogEntries", "Number of log entries", PerformanceCounterType.NumberOfItems32)
            };


            PerformanceCounterCategory.Create("LoggingCountersCategory", "Logging performance counters",
                PerformanceCounterCategoryType.SingleInstance, counterDataCollection);
        }

        LogCounter = new PerformanceCounter("LoggingCountersCategory", "LogEntries", false);

        return true;
    }


}
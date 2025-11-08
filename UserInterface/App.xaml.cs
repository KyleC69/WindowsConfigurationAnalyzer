// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  App.xaml.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.DependencyInjection;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;
using KC.WindowsConfigurationAnalyzer.UserInterface.Activation;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;
using KC.WindowsConfigurationAnalyzer.UserInterface.Models;
using KC.WindowsConfigurationAnalyzer.UserInterface.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using KC.WindowsConfigurationAnalyzer.UserInterface.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;



namespace KC.WindowsConfigurationAnalyzer.UserInterface;



// To learn more about WinUI3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                // Default Activation Handler
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Other Activation Handlers
                services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

                // Services
                services.AddSingleton<IAppNotificationService, AppNotificationService>();
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddTransient<INavigationViewService, NavigationViewService>();

                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();

                // Core Services
                services.AddSingleton<ISampleDataService, SampleDataService>();
                services.AddSingleton<IFileService, FileService>();

                // Analyzer integration (core + modules)
                services.AddWcaCore();

                // Views and ViewModels
                services.AddTransient<EventingViewModel>();
                services.AddTransient<EventingPage>();
                services.AddTransient<AnalyzerViewModel>();
                services.AddTransient<AnalyzerPage>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<ApplicationsViewModel>();
                services.AddTransient<ApplicationsPage>();
                services.AddTransient<WmiRegistryViewModel>();
                services.AddTransient<WmiRegistryPage>();
                services.AddTransient<DriversViewModel>();
                services.AddTransient<DriversPage>();
                services.AddTransient<ServicesViewModel>();
                services.AddTransient<ServicesPage>();
                services.AddTransient<ReportViewModel>();
                services.AddTransient<ReportPage>();
                services.AddTransient<ShellPage>();
                services.AddTransient<ShellViewModel>();

                // Configuration
                services.Configure<LocalSettingsOptions>(
                    context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            }).Build();

        GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }





    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static WindowEx MainWindow
    {
        get;
    } = new MainWindow();

    public static UIElement? AppTitlebar
    {
        get;
        set;
    }





    public static T GetService<T>()
        where T : class
    {
        return (Current as App)!.Host.Services.GetService(typeof(T)) is not T service
            ? throw new ArgumentException(
                $"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.")
            : service;
    }





    private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            // Append to a resilient file log
            var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            var path = Path.Combine(logDir, DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmm") + "-app.txt");
            FileActionLogSink file = new(path);
            file.Append($"{DateTimeOffset.UtcNow:O}\tApp\tUnhandledException\tError\t{e.Message}\t{e.Exception}");
        }
        catch
        {
        }

        // TODO:AI - Replace with Event Logging and reporting.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }





    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        GetService<IAppNotificationService>()
            .Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        GetService<IActivationService>().ActivateAsync(args);
    }
}
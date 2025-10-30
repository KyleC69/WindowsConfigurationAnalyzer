using CommunityToolkit.Mvvm.ComponentModel;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using KC.WindowsConfigurationAnalyzer.UserInterface.Views;
using Microsoft.UI.Xaml.Controls;
using ApplicationsPage = KC.WindowsConfigurationAnalyzer.UserInterface.Views.ApplicationsPage;

namespace KC.WindowsConfigurationAnalyzer.UserInterface.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<ReportViewModel, ReportPage>();
        Configure<ServicesViewModel, ServicesPage>();
        Configure<DriversViewModel, DriversPage>();
        Configure<Wmi_RegistryViewModel, Wmi_RegistryPage>();
        Configure<ApplicationsViewModel, ApplicationsPage>();
        Configure<SettingsViewModel, SettingsPage>();
        Configure<AnalyzerViewModel, AnalyzerPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.ContainsValue(type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}

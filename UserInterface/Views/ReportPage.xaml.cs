using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;

public sealed partial class ReportPage : Page
{
    public ReportViewModel ViewModel
    {
        get;
    }

    public ReportPage()
    {
        ViewModel = App.GetService<ReportViewModel>();
        InitializeComponent();
    }
}

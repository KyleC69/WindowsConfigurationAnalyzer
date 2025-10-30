using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;

public sealed partial class AnalyzerPage : Page
{
    public AnalyzerViewModel ViewModel
    {
        get;
    }

    public AnalyzerPage()
    {
        ViewModel = App.GetService<AnalyzerViewModel>();
        InitializeComponent();
    }
}

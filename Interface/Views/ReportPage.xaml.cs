using Interface.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Interface.Views;

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

using CommunityToolkit.WinUI.UI.Controls;
using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;

public sealed partial class ApplicationsPage : Page
{
    public ApplicationsViewModel ViewModel
    {
        get;
    }

    public ApplicationsPage()
    {
        ViewModel = App.GetService<ApplicationsViewModel>();
        InitializeComponent();
    }

    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {
        if (e == ListDetailsViewState.Both)
        {
            ViewModel.EnsureItemSelected();
        }
    }
}

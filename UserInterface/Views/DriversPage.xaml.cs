using CommunityToolkit.WinUI.UI.Controls;
using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;

public sealed partial class DriversPage : Page
{
    public DriversViewModel ViewModel
    {
        get;
    }

    public DriversPage()
    {
        ViewModel = App.GetService<DriversViewModel>();
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

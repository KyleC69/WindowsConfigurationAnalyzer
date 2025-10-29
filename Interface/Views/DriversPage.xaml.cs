using CommunityToolkit.WinUI.UI.Controls;

using Interface.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Interface.Views;

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

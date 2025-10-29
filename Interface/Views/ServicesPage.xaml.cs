using CommunityToolkit.WinUI.UI.Controls;

using Interface.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Interface.Views;

public sealed partial class ServicesPage : Page
{
    public ServicesViewModel ViewModel
    {
        get;
    }

    public ServicesPage()
    {
        ViewModel = App.GetService<ServicesViewModel>();
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

using CommunityToolkit.WinUI.UI.Controls;

using Interface.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Interface.Views;

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

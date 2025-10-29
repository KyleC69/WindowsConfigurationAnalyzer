using CommunityToolkit.WinUI.UI.Controls;

using Interface.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Interface.Views;

public sealed partial class Wmi_RegistryPage : Page
{
    public Wmi_RegistryViewModel ViewModel
    {
        get;
    }

    public Wmi_RegistryPage()
    {
        ViewModel = App.GetService<Wmi_RegistryViewModel>();
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

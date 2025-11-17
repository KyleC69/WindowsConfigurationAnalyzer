// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  WmiRegistryPage.xaml.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using CommunityToolkit.WinUI.UI.Controls;

using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

using Microsoft.UI.Xaml.Controls;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;


public sealed partial class WmiRegistryPage : Page
{


    public WmiRegistryPage()
    {
        ViewModel = App.GetService<WmiRegistryViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }





    public WmiRegistryViewModel ViewModel { get; }





    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {
        if (e == ListDetailsViewState.Both)
        {
            ViewModel.EnsureItemSelected();
        }
    }


}
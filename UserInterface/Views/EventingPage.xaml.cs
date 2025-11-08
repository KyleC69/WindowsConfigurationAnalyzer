// Created:  2025/11/04
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  EventingPage.xaml.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;



public sealed partial class EventingPage : Page
{
    public EventingPage()
    {
        ViewModel = App.GetService<EventingViewModel>();
        InitializeComponent();

        DataContext = ViewModel;

        Loaded += Page_Loaded;
    }





    public EventingViewModel ViewModel
    {
        get;
    }





    public void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.LogNames.Count == 0)
        {
            ViewModel.OnNavigatedTo(null!);
        }
    }
}
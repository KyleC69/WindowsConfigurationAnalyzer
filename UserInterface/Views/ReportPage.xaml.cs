// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  ReportPage.xaml.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using Microsoft.UI.Xaml.Controls;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;



public sealed partial class ReportPage : Page
{
    public ReportPage()
    {
        ViewModel = App.GetService<ReportViewModel>();
        InitializeComponent();
    }





    public ReportViewModel ViewModel
    {
        get;
    }
}
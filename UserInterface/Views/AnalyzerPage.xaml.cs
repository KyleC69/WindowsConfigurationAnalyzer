// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  AnalyzerPage.xaml.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

using Microsoft.UI.Xaml.Controls;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;


public sealed partial class AnalyzerPage : Page
{


    public AnalyzerPage()
    {
        ViewModel = App.GetService<AnalyzerViewModel>();
        InitializeComponent();
    }





    public AnalyzerViewModel ViewModel { get; }


}
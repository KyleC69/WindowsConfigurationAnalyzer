// Created:  2025/10/29
// Solution:
// Project:
// File:
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





	public ReportViewModel ViewModel { get; }
}
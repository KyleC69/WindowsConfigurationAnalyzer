// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  ReportViewModel.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;


public partial class ReportViewModel : ObservableRecipient
{

    public ReportViewModel()
    {
        RunRulesCommand = new AsyncRelayCommand(ExecuteRunRulesAsync);
    }

    public ICommand RunRulesCommand { get; }

    private async Task ExecuteRunRulesAsync()
    {
        var runner = new RulesRunner();
        await runner.RunRulesAsync();
    }

}
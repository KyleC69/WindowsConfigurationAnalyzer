// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  INavigationAware.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.ViewModels;


public interface INavigationAware
{


    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom();


}
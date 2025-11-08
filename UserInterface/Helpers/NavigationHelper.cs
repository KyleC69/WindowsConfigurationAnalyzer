// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  NavigationHelper.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;



// Helper class to set the navigation target for a NavigationViewItem.
//
// Usage in XAML:
// <NavigationViewItem x:Uid="Shell_Main" Icon="Document" helpers:NavigationHelper.NavigateTo="AppName.ViewModels.MainViewModel" />
//
// Usage in code:
// NavigationHelper.SetNavigateTo(navigationViewItem, typeof(MainViewModel).FullName);
public class NavigationHelper
{
    public static readonly DependencyProperty NavigateToProperty =
        DependencyProperty.RegisterAttached("NavigateTo", typeof(string), typeof(NavigationHelper),
            new PropertyMetadata(null));





    public static string GetNavigateTo(NavigationViewItem item)
    {
        return (string)item.GetValue(NavigateToProperty);
    }





    public static void SetNavigateTo(NavigationViewItem item, string value)
    {
        item.SetValue(NavigateToProperty, value);
    }
}
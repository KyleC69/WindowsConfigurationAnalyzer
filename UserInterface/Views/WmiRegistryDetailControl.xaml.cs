// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  WmiRegistryDetailControl.xaml.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;


public sealed partial class WmiRegistryDetailControl : UserControl
{


    public static readonly DependencyProperty ListDetailsMenuItemProperty =
        DependencyProperty.Register("ListDetailsMenuItem", typeof(SampleOrder), typeof(WmiRegistryDetailControl),
            new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));





    public WmiRegistryDetailControl()
    {
        InitializeComponent();
    }





    public SampleOrder? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as SampleOrder;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }





    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WmiRegistryDetailControl control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }


}
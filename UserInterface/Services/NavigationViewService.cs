// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  NavigationViewService.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Diagnostics.CodeAnalysis;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;
using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using Microsoft.UI.Xaml.Controls;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Services;



public class NavigationViewService(INavigationService navigationService, IPageService pageService)
    : INavigationViewService
{

    private NavigationView? _navigationView;





    public IList<object>? MenuItems => _navigationView?.MenuItems;

    public object? SettingsItem => _navigationView?.SettingsItem;





    [MemberNotNull(nameof(_navigationView))]
    public void Initialize(NavigationView navigationView)
    {
        _navigationView = navigationView;
        _navigationView.BackRequested += OnBackRequested;
        _navigationView.ItemInvoked += OnItemInvoked;
    }





    public void UnregisterEvents()
    {
        if (_navigationView != null)
        {
            _navigationView.BackRequested -= OnBackRequested;
            _navigationView.ItemInvoked -= OnItemInvoked;
        }
    }





    public NavigationViewItem? GetSelectedItem(Type pageType)
    {
        return _navigationView != null
            ? GetSelectedItem(_navigationView.MenuItems, pageType) ??
              GetSelectedItem(_navigationView.FooterMenuItems, pageType)
            : null;
    }





    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        navigationService.GoBack();
    }





    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        }
        else
        {
            NavigationViewItem? selectedItem = args.InvokedItemContainer as NavigationViewItem;

            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
            {
                navigationService.NavigateTo(pageKey);
            }
        }
    }





    private NavigationViewItem? GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
    {
        foreach (var item in menuItems.OfType<NavigationViewItem>())
        {
            if (IsMenuItemForPageType(item, pageType))
            {
                return item;
            }

            var selectedChild = GetSelectedItem(item.MenuItems, pageType);

            if (selectedChild != null)
            {
                return selectedChild;
            }
        }

        return null;
    }





    private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
    {
        return menuItem.GetValue(NavigationHelper.NavigateToProperty) is string pageKey &&
               pageService.GetPageType(pageKey) == sourcePageType;
    }
}
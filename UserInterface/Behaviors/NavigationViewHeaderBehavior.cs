// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  NavigationViewHeaderBehavior.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Xaml.Interactivity;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Behaviors;



public class NavigationViewHeaderBehavior : Behavior<NavigationView>
{
    private static NavigationViewHeaderBehavior? _current;

    public static readonly DependencyProperty DefaultHeaderProperty =
        DependencyProperty.Register("DefaultHeader", typeof(object), typeof(NavigationViewHeaderBehavior),
            new PropertyMetadata(null!, (d, e) => _current!.UpdateHeader()));


    public static readonly DependencyProperty HeaderModeProperty =
        DependencyProperty.RegisterAttached("HeaderMode", typeof(bool), typeof(NavigationViewHeaderBehavior),
            new PropertyMetadata(NavigationViewHeaderMode.Always, (d, e) => _current!.UpdateHeader()));


    private static readonly DependencyProperty HeaderContextProperty =
        DependencyProperty.RegisterAttached("HeaderContext", typeof(object), typeof(NavigationViewHeaderBehavior),
            new PropertyMetadata(null, (d, e) => _current!.UpdateHeader()));


    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.RegisterAttached("HeaderTemplate", typeof(DataTemplate),
            typeof(NavigationViewHeaderBehavior),
            new PropertyMetadata(null, (d, e) => _current!.UpdateHeaderTemplate()));

    private Page? _currentPage;

    public DataTemplate? DefaultHeaderTemplate
    {
        get;
        set;
    }



    public object DefaultHeader
    {
        get => GetValue(DefaultHeaderProperty);
        set => SetValue(DefaultHeaderProperty, value);
    }





    public static NavigationViewHeaderMode GetHeaderMode(Page item) =>
        (NavigationViewHeaderMode)item.GetValue(HeaderModeProperty);





    public static void SetHeaderMode(Page item, NavigationViewHeaderMode value)
    {
        item.SetValue(HeaderModeProperty, value);
    }





    public static object GetHeaderContext(Page item) => item.GetValue(HeaderContextProperty);





    public static void SetHeaderContext(Page item, object value)
    {
        item.SetValue(HeaderContextProperty, value);
    }





    public static DataTemplate GetHeaderTemplate(Page item) => (DataTemplate)item.GetValue(HeaderTemplateProperty);





    public static void SetHeaderTemplate(Page item, DataTemplate value)
    {
        item.SetValue(HeaderTemplateProperty, value);
    }





    protected override void OnAttached()
    {
        base.OnAttached();

        INavigationService navigationService = App.GetService<INavigationService>();
        navigationService.Navigated += OnNavigated;

        _current = this;
    }





    protected override void OnDetaching()
    {
        base.OnDetaching();

        INavigationService navigationService = App.GetService<INavigationService>();
        navigationService.Navigated -= OnNavigated;
    }





    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame && frame.Content is Page page)
        {
            _currentPage = page;

            UpdateHeader();
            UpdateHeaderTemplate();
        }
    }





    private void UpdateHeader()
    {
        if (_currentPage != null)
        {
            NavigationViewHeaderMode headerMode = GetHeaderMode(_currentPage);
            if (headerMode == NavigationViewHeaderMode.Never)
            {
                AssociatedObject.Header = null;
                AssociatedObject.AlwaysShowHeader = false;
            }
            else
            {
                var headerFromPage = GetHeaderContext(_currentPage);
                AssociatedObject.Header = headerFromPage;
                AssociatedObject.AlwaysShowHeader = headerMode == NavigationViewHeaderMode.Always;
            }
        }
    }






    private void UpdateHeaderTemplate()
    {
        if (_currentPage != null)
        {
            DataTemplate? headerTemplate = GetHeaderTemplate(_currentPage);
            AssociatedObject.HeaderTemplate = headerTemplate ?? DefaultHeaderTemplate;
        }
    }
}
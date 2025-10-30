namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.ViewModels;

public interface INavigationAware
{
    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom();
}

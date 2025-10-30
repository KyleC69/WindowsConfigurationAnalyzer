namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}

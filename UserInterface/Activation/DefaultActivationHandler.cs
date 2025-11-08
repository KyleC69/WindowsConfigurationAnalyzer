// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using Microsoft.UI.Xaml;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Activation;



public class DefaultActivationHandler(INavigationService navigationService)
	: ActivationHandler<LaunchActivatedEventArgs>
{





	protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
	{
		// None of the ActivationHandlers has handled the activation.
		return navigationService.Frame?.Content == null;
	}





	protected override  Task HandleInternalAsync(LaunchActivatedEventArgs args)
	{
		navigationService.NavigateTo(typeof(ReportViewModel).FullName!, args.Arguments);

		return Task.CompletedTask;
	}
}
// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Activation;



public interface IActivationHandler
{
	bool CanHandle(object args);

	Task HandleAsync(object args);
}
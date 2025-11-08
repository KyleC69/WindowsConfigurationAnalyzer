// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Collections.Specialized;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;



public interface IAppNotificationService
{
	void Initialize();

	bool Show(string payload);

	NameValueCollection ParseArguments(string arguments);

	void Unregister();
}
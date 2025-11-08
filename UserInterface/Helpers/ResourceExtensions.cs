// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using Microsoft.Windows.ApplicationModel.Resources;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;



public static class ResourceExtensions
{
	private static readonly ResourceLoader ResourceLoader = new();





	public static string GetLocalized(this string resourceKey)
	{
		return ResourceLoader.GetString(resourceKey);
	}
}
// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using Newtonsoft.Json;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers;



public static class Json
{
	public static async Task<T?> ToObjectAsync<T>(string value)
	{
		return await Task.Run<T>(() => { return JsonConvert.DeserializeObject<T>(value); });
	}





	public static async Task<string> StringifyAsync(object value)
	{
		return await Task.Run<string>(() => { return JsonConvert.SerializeObject(value); });
	}
}
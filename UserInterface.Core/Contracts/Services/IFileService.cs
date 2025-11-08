// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;



public interface IFileService
{
	T? Read<T>(string folderPath, string fileName);

	void Save<T>(string folderPath, string fileName, T content);

	void Delete(string folderPath, string fileName);
}
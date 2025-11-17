// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface.Core
// File:  ISampleDataService.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;


// Remove this class once your pages/features are using your data.
public interface ISampleDataService
{


    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();


}
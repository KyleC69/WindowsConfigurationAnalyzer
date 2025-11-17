// Created:  2025/11/16
// Solution: WindowsConfigurationAnalyzer
// Project:  Contracts
// File:  IAnalyzerModule.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.Contracts.Models;





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IAnalyzerModule
{


    string Name { get; }

    string Area { get; }

    Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken);


}
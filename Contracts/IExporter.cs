// Created:  2025/11/16
// Solution: WindowsConfigurationAnalyzer
// Project:  Contracts
// File:  IExporter.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.Contracts.Models;





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IExporter
{


    Task ExportAsync(AnalyzerResult result, string targetPath, CancellationToken cancellationToken);


}
// Created:  2025/11/16
// Solution: WindowsConfigurationAnalyzer
// Project:  Contracts
// File:  IAnomalyDetector.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder

using KC.WindowsConfigurationAnalyzer.Contracts.Models;





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IAnomalyDetector
{


    IReadOnlyList<Finding> Detect(AnalyzerResult result);


}
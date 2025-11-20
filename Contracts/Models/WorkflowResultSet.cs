// Created:  2025/11/17
// Solution: WindowsConfigurationAnalyzer
// Project:  Contracts
// File:  WorkflowResultSet.cs
// 
// All Rights Reserved 2025
// Author: Kyle L Crowder




namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;


public class WorkflowResultSet
{
    public string WorkflowName { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartedOn { get; set; }
    public DateTime CompletedOn { get; set; }
    public List<RuleResult> Results { get; set; } = new();
}
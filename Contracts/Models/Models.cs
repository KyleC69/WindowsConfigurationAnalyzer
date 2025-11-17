// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  Models.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder

namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;


public sealed record AnalyzerResult(
    string ComputerName,
    DateTimeOffset ExportTimestampUtc,
    IReadOnlyList<AreaResult> Areas,
    IReadOnlyList<Finding> GlobalFindings
);



public sealed record AreaResult(
    string Area,
    object? Summary,
    object? Details,
    IReadOnlyList<Finding> Anomalies,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors);



public sealed record ActionLogEntry(
    DateTimeOffset TimestampUtc,
    string Area,
    string Action,
    string Level,
    string Message,
    string? Exception);



public sealed record Finding(
    string Severity, // Info | Warning | Critical
    string Message,
    string? Context = null);
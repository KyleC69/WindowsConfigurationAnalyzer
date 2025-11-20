// Created:  2025/11/17
// Solution: WindowsConfigurationAnalyzer
// Project:  RuleAnalyzer
// File:  SampleRule.cs
// 
// All Rights Reserved 2025
// Author: Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.Contracts;





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.docs;


public class SampleRule
{
    void SampleRuleMethod()
    {
        /*
        var registryRule = new RuleContract
        RuleName = "CheckProfileRegistryKey",
        Author = "Kyle",
        Category = "Configuration",
        Tags = new List<string> { "Registry", "Profile" },
        Parameters = new List<RuleParameter>
        {
            new RuleParameter { Name = "RegistryProfileKey", Type = ParameterType.String }
        },
        Probe = new ProbeTarget
        {
            Subsystem = SubsystemType.Registry,
            Location = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList",
            Pattern = @"S-1-5-21.*"
        },
        Expression = new ExpressionDefinition
        {
            Type = ExpressionType.FunctionCall,
            ExpressionText = "Registry.Exists(RegistryProfileKey)"
        },
        SuccessMessage = "Profile registry key exists.",
        FailureMessage = "Profile registry key missing.",
        OnSuccess = new List<RuleAction>
        {
            new RuleAction { ActionType = ActionType.Log, Target = "audit.log" }
        },
        OnFailure = new List<RuleAction>
        {
            new RuleAction { ActionType = ActionType.Notify, Target = "admin@domain.com" }
        },
        Constraints = new ExecutionConstraints
        {
            RunOncePerSession = true,
            Timeout = TimeSpan.FromSeconds(10)
        },
        SeverityScore = 5,
        EnableAuditTrail = true
    
            */
}

}
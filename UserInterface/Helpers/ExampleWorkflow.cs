using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;







namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

public static class ExampleWorkflow
{







    public static WorkflowContract GetSampleWorkflow()
    {

       return new WorkflowContract
        {
            WorkflowName = "ProfileValidationWorkflow",
            Author = "Kyle",

            GlobalParameters = new WorkflowParameters
            {
                ProviderName   = "ProfileProvider",
                ManifestPath   = @"C:\Manifests\profile.man",
                ProbeTimestamp = DateTime.UtcNow
            },

            Rules = new List<RuleContract>
    {
        new RuleContract
        {
            RuleName   = "CheckProfileRegistryKey",
            Parameters = new List<RuleParameter>
            {
                new RuleParameter
                {
                    Name = "RegistryProfileKey",
                    Type = ParameterType.String
                }
            },
            Probe = new ProbeTarget
            {
                Subsystem = SubsystemType.Registry,
                Location  = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList",
                Pattern   = @"S-1-5-21.*"
            },
            Expression = new ExpressionDefinition
            {
                ExpressionType = ExpressionType.FunctionCall,
                ExpressionText = "Registry.Exists(RegistryProfileKey)"
            },
            SuccessMessage = "Profile registry key exists.",
            FailureMessage = "Profile registry key missing."
        },

        new RuleContract
        {
            RuleName   = "CheckLocalProfilePath",
            Parameters = new List<RuleParameter>
            {
                new RuleParameter
                {
                    Name = "ProfilePath",
                    Type = ParameterType.String
                }
            },
            Probe = new ProbeTarget
            {
                Subsystem = SubsystemType.FileSystem,
                Location  = @"C:\Users",
                Pattern   = @"*"
            },
            Expression = new ExpressionDefinition
            {
                ExpressionType = ExpressionType.FunctionCall,
                ExpressionText = "Directory.Exists(ProfilePath)"
            },
            SuccessMessage = "Local profile path exists.",
            FailureMessage = "Local profile path missing."
        }
    },

            Constraints = new WorkflowConstraints
            {
                RunSequentially = true,
                StopOnFailure   = false
            }
        };





    }




}

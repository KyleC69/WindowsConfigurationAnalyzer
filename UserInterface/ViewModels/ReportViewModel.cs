// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  ReportViewModel.cs
// 
// All Rights Reserved 2025
// Author: Kyle L Crowder






using System.Collections.ObjectModel;

using Json = Newtonsoft.Json.JsonSerializer;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KC.WindowsConfigurationAnalyzer.Contracts.Models;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

using Newtonsoft.Json;

using RulesEngine.Models;
using System.Text.Json;






namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;


public partial class ReportViewModel : ObservableRecipient
{

    private static readonly string ProjectDir = App.ProjectDir ?? @"D:\\Solutions\\KC.WindowsConfigurationAnalyzer";
    public string RULESTOREFOLDER = Path.Combine(ProjectDir, "RulesEngineStore");


    public ReportViewModel()
    {
        RunRulesCommand = new AsyncRelayCommand(ExecuteRunRulesAsync);
        LoadRulesCollection();
    }


    public ObservableCollection<RuleItem> RuleListCollection { get; } = new();
    public ObservableCollection<WorkflowContract> WorkFlows { get; } = new();
    public AsyncRelayCommand RunRulesCommand { get; }





    private void LoadRulesCollection()
    {
        //Read all the json files in the rule store and populate the Rule collection
        var sample = ExampleWorkflow.GetSampleWorkflow();

        try
        {
            var rulesFiles = Directory.GetFiles(RULESTOREFOLDER, "*.json", SearchOption.AllDirectories);

            foreach (var file in rulesFiles)
            {
                if (File.Exists(file))
                {
                    var ruleFileContent = File.ReadAllText(file);
                    var ruleJson = JsonConvert.DeserializeObject<WorkflowContract>(ruleFileContent);
                    if (ruleJson != null) WorkFlows.Add(ruleJson);


                    // For testing purposes only- write out a sample workflow file
                    // var stringjson= JsonSerializer.Serialize<WorkflowContract>(sample, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    //  File.WriteAllText(Path.Combine(RULESTOREFOLDER, "sampleworkflow.json"), stringjson);

                }
            }
        }
        catch (Exception e)
        {
            ActivityLogger.Log("ERR", $"Failure attempting to load workflows from file:: {e.Message}", "ReportViewModel::LoadRulesCollection");

        }
    }





    private async Task ExecuteRunRulesAsync()
    {
        var eng = new WorkflowEngine();

        var results =   await eng.ExecuteWorkflowAsync(ExampleWorkflow.GetSampleWorkflow());




    }


}



public record RuleItem
{


    public string RuleName { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = false;

    //For ease of loading rules from file- Not intended to be bound to UI
    public string FileName { get; set; } = string.Empty;


}
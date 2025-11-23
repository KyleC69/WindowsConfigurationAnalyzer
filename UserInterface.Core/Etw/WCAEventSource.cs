//  Created:  2025/11/01
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface.Core
//        File:   WCAEventSource.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Diagnostics.Tracing;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Etw;


[EventSource(Name = "WCA-Provider", Guid = "6bfde06a-0e76-53be-3771-2e44fb5386ee")]
public sealed class WCAEventSource : EventSource
{


    public static readonly WCAEventSource Log = new();





    // --- Core lifecycle (Operational channel) ---
    // Added constructor to initialize EventSource.
    private WCAEventSource()
    {
    }









    [Event(2002, Level = EventLevel.Informational, Task = Tasks.Analysis, Opcode = EventOpcode.Start, Keywords = Keywords.Analyzer, Channel = EventChannel.Operational)]
    public void AnalyzeAreaStart(string SessionId, string Area, string CorrelationId)
    {
        WriteEvent(2002, SessionId, Area, CorrelationId);
    }





    [Event(2003, Level = EventLevel.Informational, Task = Tasks.Analysis, Opcode = EventOpcode.Info, Keywords = Keywords.Analyzer, Channel = EventChannel.Operational)]
    public void AnalyzeAreaComplete(string SessionId, string Area, uint FindingsCount, string Severity, string Summary, string CorrelationId)
    {
        WriteEvent(2003, SessionId, Area, FindingsCount, Severity, Summary, CorrelationId);
    }





    [Event(2004, Level = EventLevel.Informational, Task = Tasks.Complete, Opcode = EventOpcode.Info, Keywords = Keywords.Analyzer, Channel = EventChannel.Operational)]
    public void AnalyzeComplete(string SessionId, uint Areas, uint Warnings, uint Errors, double ElapsedSeconds, string CorrelationId)
    {
        WriteEvent(2004, SessionId, Areas, Warnings, Errors, ElapsedSeconds, CorrelationId);
    }





    // Security events
    [Event(2101, Level = EventLevel.Informational, Task = Tasks.Discovery, Opcode = EventOpcode.Extension, Keywords = Keywords.Security, Channel = EventChannel.Operational)]
    public void SecuritySettingDiscovered(string SessionId, string SettingName, string Value, string CorrelationId)
    {
        WriteEvent(2101, SessionId, SettingName, Value, CorrelationId);
    }





    [Event(2102, Level = EventLevel.Warning, Task = Tasks.Analysis, Opcode = Opcodes.SecurityWarn, Keywords = Keywords.Security, Channel = EventChannel.Operational)]
    public void SecurityConfigurationWarning(string SessionId, string SettingName, string WarningMessage, string CorrelationId)
    {
        WriteEvent(2102, SessionId, SettingName, WarningMessage, CorrelationId);
    }





    // Performance events
    [Event(2201, Level = EventLevel.Informational, Task = Tasks.Discovery, Opcode = Opcodes.PerfDiscovered, Keywords = Keywords.Performance, Channel = EventChannel.Operational)]
    public void PerformanceCounterDiscovered(string SessionId, string CounterName, string Value, string CorrelationId)
    {
        WriteEvent(2201, SessionId, CounterName, Value, CorrelationId);
    }





    [Event(2202, Level = EventLevel.Warning, Task = Tasks.Analysis, Opcode = Opcodes.PerfIssue, Keywords = Keywords.Performance, Channel = EventChannel.Operational)]
    public void PerformanceIssueDetected(string SessionId, string CounterName, string IssueMessage, string CorrelationId)
    {
        WriteEvent(2202, SessionId, CounterName, IssueMessage, CorrelationId);
    }





    // Integration events
    [Event(2301, Level = EventLevel.Informational, Task = Tasks.Integration, Opcode = Opcodes.IntegrationPoint, Keywords = Keywords.Integration, Channel = EventChannel.Operational)]
    public void IntegrationPointDiscovered(string SessionId, string IntegrationName, string Status, string CorrelationId)
    {
        WriteEvent(2301, SessionId, IntegrationName, Status, CorrelationId);
    }





    [Event(2302, Level = EventLevel.Error, Task = Tasks.Integration, Opcode = Opcodes.IntegrationFailure, Keywords = Keywords.Integration, Channel = EventChannel.Operational)]
    public void IntegrationFailed(string SessionId, string IntegrationName, string ErrorMessage, string CorrelationId)
    {
        WriteEvent(2302, SessionId, IntegrationName, ErrorMessage, CorrelationId);
    }





    // Reader events
    [Event(2401, Level = EventLevel.Informational, Task = Tasks.Discovery, Opcode = Opcodes.ReaderDiscovered, Keywords = Keywords.Readers, Channel = EventChannel.Operational)]
    public void ReaderDiscovered(string SessionId, string ReaderName, string Status, string CorrelationId)
    {
        WriteEvent(2401, SessionId, ReaderName, Status, CorrelationId);
    }






    [Event(2402, Level = EventLevel.Error, Task = Tasks.Discovery, Opcode = Opcodes.ReaderFailure, Keywords = Keywords.Readers, Channel = EventChannel.Operational)]
    public void ReaderError(string SessionId, string ReaderName, string ErrorMessage, string CorrelationId)
    {
        WriteEvent(2402, SessionId, ReaderName, ErrorMessage, CorrelationId);
    }





    // Diagnostics events
    [Event(2501, Level = EventLevel.Informational, Task = Tasks.Discovery, Opcode = Opcodes.DiagnosticInfo, Keywords = Keywords.Diagnostics, Channel = EventChannel.Operational)]
    public void DiagnosticInfoFound(string SessionId, string InfoType, string Details, string CorrelationId)
    {
        WriteEvent(2501, SessionId, InfoType, Details, CorrelationId);
    }





    [Event(2502, Level = EventLevel.Error, Task = Tasks.Discovery, Opcode = Opcodes.DiagnosticFailure, Keywords = Keywords.Diagnostics, Channel = EventChannel.Operational)]
    public void DiagnosticError(string SessionId, string ErrorType, string ErrorMessage, string CorrelationId)
    {
        WriteEvent(2502, SessionId, ErrorType, ErrorMessage, CorrelationId);
    }





    // UI events
    [Event(2601, Level = EventLevel.Informational, Task = Tasks.Control, Opcode = Opcodes.UINotify, Keywords = Keywords.UI, Channel = EventChannel.Admin, Message = "User Interface Notification: {0}")]
    public void UINotification(string SessionId, string Message, string CorrelationId)
    {
        WriteEvent(2601, SessionId, Message, CorrelationId);
    }





    [Event(2602, Level = EventLevel.Warning, Task = Tasks.Control, Opcode = Opcodes.UIWarn, Keywords = Keywords.UI, Channel = EventChannel.Operational)]
    public void UIWarning(string SessionId, string Message, string CorrelationId)
    {
        WriteEvent(2602, SessionId, Message, CorrelationId);
    }





    // Database events
    [Event(2701, Level = EventLevel.Informational, Task = Tasks.Discovery, Opcode = Opcodes.DatabaseEntry, Keywords = Keywords.Database, Channel = EventChannel.Operational)]
    public void DatabaseEntryDiscovered(string SessionId, string TableName, string Key, string CorrelationId)
    {
        WriteEvent(2701, SessionId, TableName, Key, CorrelationId);
    }





    [Event(2702, Level = EventLevel.Error, Task = Tasks.Discovery, Opcode = Opcodes.DatabaseFailure, Keywords = Keywords.Database, Channel = EventChannel.Operational)]
    public void DatabaseError(string SessionId, string TableName, string ErrorMessage, string CorrelationId)
    {
        WriteEvent(2702, SessionId, TableName, ErrorMessage, CorrelationId);
    }





    // Audit events
    [Event(2801, Level = EventLevel.Informational, Task = Tasks.Discovery, Opcode = Opcodes.AuditTrail, Keywords = Keywords.Audit, Channel = EventChannel.Operational)]
    public void AuditTrailEntryFound(string SessionId, string EventType, string Details, string CorrelationId)
    {
        WriteEvent(2801, SessionId, EventType, Details, CorrelationId);
    }





    [Event(2802, Level = EventLevel.Warning, Task = Tasks.Discovery, Opcode = Opcodes.AuditWarn, Keywords = Keywords.Audit, Channel = EventChannel.Operational)]
    public void AuditWarning(string SessionId, string EventType, string WarningMessage, string CorrelationId)
    {
        WriteEvent(2802, SessionId, EventType, WarningMessage, CorrelationId);
    }





    // Network events
    [Event(2901, Level = EventLevel.Informational, Task = Tasks.Discovery, Opcode = Opcodes.NetworkConnection, Keywords = Keywords.Network, Channel = EventChannel.Operational)]
    public void NetworkConnectionDiscovered(string SessionId, string Protocol, string Address, int Port, string CorrelationId)
    {
        WriteEvent(2901, SessionId, Protocol, Address, Port, CorrelationId);
    }





    [Event(2902, Level = EventLevel.Error, Task = Tasks.Discovery, Opcode = Opcodes.NetworkFailure, Keywords = Keywords.Network, Channel = EventChannel.Operational)]
    public void NetworkError(string SessionId, string Protocol, string Address, int Port, string ErrorMessage, string CorrelationId)
    {
        WriteEvent(2902, SessionId, Protocol, Address, Port, ErrorMessage, CorrelationId);
    }





    // OS events
    [Event(3001, Level = EventLevel.Informational, Task = Tasks.Discovery, Opcode = Opcodes.OSInfo, Keywords = Keywords.OS, Channel = EventChannel.Operational)]
    public void OSInfoDiscovered(string SessionId, string InfoType, string Details, string CorrelationId)
    {
        WriteEvent(3001, SessionId, InfoType, Details, CorrelationId);
    }





    [Event(3002, Level = EventLevel.Error, Task = Tasks.Discovery, Opcode = Opcodes.OSFailure, Keywords = Keywords.OS, Channel = EventChannel.Operational)]
    public void OSError(string SessionId, string ErrorType, string ErrorMessage, string CorrelationId)
    {
        WriteEvent(3002, SessionId, ErrorType, ErrorMessage, CorrelationId);
    }





    // Configuration events
    [Event(3101, Level = EventLevel.Informational, Task = Tasks.Discovery, Opcode = Opcodes.ConfigSetting, Keywords = Keywords.Configuration, Channel = EventChannel.Operational)]
    public void ConfigurationSettingDiscovered(string SessionId, string SettingName, string Value, string CorrelationId)
    {
        WriteEvent(3101, SessionId, SettingName, Value, CorrelationId);
    }





    [Event(3102, Level = EventLevel.Warning, Task = Tasks.Discovery, Opcode = Opcodes.ConfigWarn, Keywords = Keywords.Configuration, Channel = EventChannel.Operational)]
    public void ConfigurationWarning(string SessionId, string SettingName, string WarningMessage, string CorrelationId)
    {
        WriteEvent(3102, SessionId, SettingName, WarningMessage, CorrelationId);
    }





    [Event(3103, Level = EventLevel.Error, Task = Tasks.Discovery, Opcode = Opcodes.ConfigFailure, Keywords = Keywords.Configuration, Channel = EventChannel.Operational)]
    public void ConfigurationError(string SessionId, string SettingName, string ErrorMessage, string CorrelationId)
    {
        WriteEvent(3103, SessionId, SettingName, ErrorMessage, CorrelationId);
    }





    // Rules Engine events
    [Event(4301, Level = EventLevel.Informational, Task = Tasks.RulesEngine, Opcode = Opcodes.Execute, Keywords = Keywords.RulesEngine, Channel = EventChannel.Operational)]
    public void RuleExecutionStart(string SessionId, string RuleName, string CorrelationId)
    {
        WriteEvent(4301, SessionId, RuleName, CorrelationId);
    }





    [Event(4302, Level = EventLevel.Informational, Task = Tasks.RulesEngine, Opcode = Opcodes.RuleResult, Keywords = Keywords.RulesEngine, Channel = EventChannel.Operational)]
    public void RuleExecutionResult(string SessionId, string RuleName, bool Success, string Message, string CorrelationId)
    {
        WriteEvent(4302, SessionId, RuleName, Success, Message, CorrelationId);
    }





    [Event(4303, Level = EventLevel.Error, Task = Tasks.RulesEngine, Opcode = Opcodes.RuleError, Keywords = Keywords.RulesEngine, Channel = EventChannel.Operational)]
    public void RuleExecutionError(string SessionId, string RuleName, string ErrorMessage, string CorrelationId)
    {
        WriteEvent(4303, SessionId, RuleName, ErrorMessage, CorrelationId);
    }





    [Event(4304, Level = EventLevel.Informational, Task = Tasks.RulesEngine, Opcode = Opcodes.RuleSummary, Keywords = Keywords.RulesEngine, Channel = EventChannel.Operational)]
    public void RulesEngineSummary(string SessionId, uint RulesExecuted, uint RulesPassed, uint RulesFailed, string CorrelationId)
    {
        WriteEvent(4304, SessionId, RulesExecuted, RulesPassed, RulesFailed, CorrelationId);
    }





    // --- Core lifecycle (Operational channel) ---
    [Event(1001, Level = EventLevel.Informational, Task = Tasks.Control, Opcode = EventOpcode.Start,
        Keywords = Keywords.Analyzer, Channel = EventChannel.Operational)]
    public void SessionStart(string SessionId, string Computer, string Version, string CorrelationId)
    {
        WriteEvent(1001, SessionId, Computer, Version, CorrelationId);
    }





    [Event(1002, Level = EventLevel.Informational, Task = Tasks.Control, Opcode = EventOpcode.Stop,
        Keywords = Keywords.Analyzer, Channel = EventChannel.Operational)]
    public void SessionStop(string SessionId, int Areas, int Warnings, int Errors, double ElapsedSeconds, string CorrelationId)
    {
        WriteEvent(1002, SessionId, Areas, Warnings, Errors, ElapsedSeconds, CorrelationId);
    }





    // Exception events for catch blocks
    [Event(1003, Level = EventLevel.Error, Task = Tasks.Error, Opcode = Opcodes.ExceptionFailure,
        Keywords = Keywords.Analyzer, Channel = EventChannel.Operational)]
    public void ExceptionError(string SessionId, string CorrelationId, string ExceptMessage, string? ExceptStack, string Context)
    {
        WriteEvent(1003, SessionId, CorrelationId, ExceptMessage, ExceptStack, Context);
    }





    [Event(1004, Level = EventLevel.Warning, Task = Tasks.Warning, Opcode = EventOpcode.Info,
        Keywords = Keywords.Analyzer, Channel = EventChannel.Operational)]
    public void ExceptionWarning(string SessionId, string CorrelationId, string ExceptMessage, string ExceptStack, string Context)
    {
        WriteEvent(1004, SessionId, CorrelationId, ExceptMessage, ExceptStack, Context);
    }








    // --- Startup/Autoruns (3200–3399) ---
    [Event(3201, Level = EventLevel.Informational, Task = Tasks.Module, Opcode = Opcodes.Capture,
        Keywords = Keywords.Diagnostics, Channel = EventChannel.Operational)]
    public void StartupCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        WriteEvent(3201, Module, CorrelationId, ItemCount, DurationMs, Details);
    }





    [Event(3202, Level = EventLevel.Informational, Task = Tasks.Module, Opcode = Opcodes.Analyze,
        Keywords = Keywords.Diagnostics, Channel = EventChannel.Operational)]
    public void StartupAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity, string Summary)
    {
        WriteEvent(3202, Module, CorrelationId, FindingsCount, Severity, Summary);
    }





    // --- Audit/EventLog (3400–3599) ---
    [Event(3401, Level = EventLevel.Informational, Task = Tasks.Module, Opcode = Opcodes.AuditCaptureOp,
        Keywords = Keywords.Audit, Channel = EventChannel.Operational)]
    public void AuditCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        WriteEvent(3401, Module, CorrelationId, ItemCount, DurationMs, Details);
    }





    [Event(3402, Level = EventLevel.Informational, Task = Tasks.Module, Opcode = Opcodes.AuditAnalyzeOp,
        Keywords = Keywords.Audit, Channel = EventChannel.Operational)]
    public void AuditAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity, string Summary)
    {
        WriteEvent(3402, Module, CorrelationId, FindingsCount, Severity, Summary);
    }





    // --- Exporters (3600–3799) ---
    [Event(3601, Level = EventLevel.Informational, Task = Tasks.Export, Opcode = Opcodes.Capture,
        Keywords = Keywords.Export, Channel = EventChannel.Operational)]
    public void ExportCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        WriteEvent(3601, Module, CorrelationId, ItemCount, DurationMs, Details);
    }





    [Event(3602, Level = EventLevel.Informational, Task = Tasks.Export, Opcode = Opcodes.Analyze,
        Keywords = Keywords.Export, Channel = EventChannel.Operational)]
    public void ExportAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity, string Summary)
    {
        WriteEvent(3602, Module, CorrelationId, FindingsCount, Severity, Summary);
    }





    [Event(3610, Level = EventLevel.Informational, Task = Tasks.Export, Opcode = EventOpcode.Info,
        Keywords = Keywords.Export, Channel = EventChannel.Operational)]
    public void ExportCompleted(string SessionId, string Format, string Path, string CorrelationId)
    {
        WriteEvent(3610, SessionId, Format, Path, CorrelationId);
    }






    // --- Readers / IO (3800–3999) ---
    [Event(3801, Level = EventLevel.Informational, Task = Tasks.Module, Opcode = Opcodes.ReadersCaptureOp,
        Keywords = Keywords.Readers, Channel = EventChannel.Operational)]
    public void ReadersCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        WriteEvent(3801, Module, CorrelationId, ItemCount, DurationMs, Details);
    }





    [Event(3802, Level = EventLevel.Informational, Task = Tasks.Module, Opcode = Opcodes.ReadersAnalyzeOp,
        Keywords = Keywords.Readers, Channel = EventChannel.Operational)]
    public void ReadersAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity, string Summary)
    {
        WriteEvent(3802, Module, CorrelationId, FindingsCount, Severity, Summary);
    }





    [Event(4001, Level = EventLevel.Error, Task = Tasks.Error, Opcode = Opcodes.ActivityLoggerFallbackOp, Keywords = Keywords.UI, Channel = EventChannel.Operational)]
    public void ActivityLoggerFallback(string error)
    {
        WriteEvent(4001, error);
    }
    // ###### Events for Rules Engine errors 4300-4399  ########





    [Event(4300, Level = EventLevel.Error, Task = Tasks.Error, Opcode = Opcodes.RulesEngineFailure, Keywords = Keywords.Analyzer, Channel = EventChannel.Operational)]
    public void RulesEngineError(string noResultsFromRuleExecution)
    {
        WriteEvent(4300, noResultsFromRuleExecution);
    }





    [Event(4215, Level = EventLevel.Informational, Task = Tasks.ActionRelated, Opcode = EventOpcode.Start, Keywords = Keywords.Analyzer, Channel = EventChannel.Operational)]
    public void ActionStartWithRelated(string message)
    {
        if (!IsEnabled())
        {
            return;
        }

        // Get the parent activity ID from the thread's current activity
        // This assumes ActionStart was called previously on this thread
        Guid parentActivityId;
        parentActivityId = CurrentThreadActivityId;

        // Establish the new activity for this thread
        Guid childActivityId = Guid.NewGuid();
        SetCurrentThreadActivityId(childActivityId);

        // Log start, linking to parent
        WriteEventWithRelatedActivityId(4215, parentActivityId, message);
    }





    [Event(4220, Level = EventLevel.Informational, Task = Tasks.Action, Opcode = EventOpcode.Stop, Keywords = Keywords.Analyzer, Channel = EventChannel.Operational)]
    public void ActionStop(string message)
    {
        // CurrentThreadActivityId is already set
        WriteEvent(4220, message);
    }





    [Event(4210, Level = EventLevel.Informational, Task = Tasks.Action, Opcode = EventOpcode.Start, Keywords = Keywords.Action, Channel = EventChannel.Operational)]
    public void ActionStart(string message)
    {
        SetCurrentThreadActivityId(Guid.NewGuid());
        WriteEvent(4210, message);
    }





    [Event(4211, Level = EventLevel.Error, Task = Tasks.Error, Opcode = Opcodes.ActionFailure, Keywords = Keywords.Action, Channel = EventChannel.Operational)]
    public void ActionFailed(string appInitializationHasFailed, string message)
    {
        WriteEvent(4211, appInitializationHasFailed, message);
    }





    // Keywords (match manifest bitmasks)
    public static class Keywords
    {


        internal const EventKeywords Analyzer = (EventKeywords)0x0000000000000001;
        internal const EventKeywords Security = (EventKeywords)0x0000000000000002;
        internal const EventKeywords Performance = (EventKeywords)0x0000000000000004;
        internal const EventKeywords Export = (EventKeywords)0x0000000000000008;
        internal const EventKeywords Integration = (EventKeywords)0x0000000000000010;
        internal const EventKeywords Readers = (EventKeywords)0x0000000000000020;
        internal const EventKeywords Diagnostics = (EventKeywords)0x0000000000000040;
        internal const EventKeywords UI = (EventKeywords)0x0000000000000080;
        internal const EventKeywords Database = (EventKeywords)0x0000000000000100;
        internal const EventKeywords Audit = (EventKeywords)0x0000000000000200;
        internal const EventKeywords Network = (EventKeywords)0x0000000000000400;
        internal const EventKeywords OS = (EventKeywords)0x0000000000000800;
        internal const EventKeywords Configuration = (EventKeywords)0x0000000000001000;
        internal const EventKeywords RulesEngine = (EventKeywords)0x0000000000002000;
        internal const EventKeywords Action = (EventKeywords)0x0000000000004000;


    }



    // Tasks (match manifest values)
    public static class Tasks
    {


        internal const EventTask Control = (EventTask)1;
        internal const EventTask Discovery = (EventTask)2;
        internal const EventTask Analysis = (EventTask)3;
        internal const EventTask Warning = (EventTask)4;
        internal const EventTask Error = (EventTask)5;
        internal const EventTask Export = (EventTask)6;
        internal const EventTask Integration = (EventTask)7;
        internal const EventTask Module = (EventTask)8;
        internal const EventTask RulesEngine = (EventTask)9;

        internal const EventTask Complete = (EventTask)10;

        // Added to disambiguate Action activity events from Control start/stop
        internal const EventTask Action = (EventTask)11;

        // Separate task for related Action activity to avoid duplicate Task/Opcode pair with ActionStart
        internal const EventTask ActionRelated = (EventTask)12;


    }



    // Custom opcodes (reserved opcodes Start/Stop/Info are built-in)
    public static class Opcodes
    {


        internal const EventOpcode Analyze = (EventOpcode)11;
        internal const EventOpcode Capture = (EventOpcode)12;
        internal const EventOpcode Export = (EventOpcode)13;
        internal const EventOpcode Execute = (EventOpcode)14;
        internal const EventOpcode RuleExecute = (EventOpcode)15;
        internal const EventOpcode RuleResult = (EventOpcode)16;
        internal const EventOpcode RuleError = (EventOpcode)17;
        internal const EventOpcode RuleSummary = (EventOpcode)18;
        internal const EventOpcode ActionStart = (EventOpcode)19;
        internal const EventOpcode ActionStop = (EventOpcode)20;
        internal const EventOpcode Analysis = (EventOpcode)22; // Changed from 3 to avoid conflict
        internal const EventOpcode Import = (EventOpcode)23;
        internal const EventOpcode Exported = (EventOpcode)24;

        internal const EventOpcode ExportFailed = (EventOpcode)25;

        // Unique opcodes to avoid duplicate Task/Opcode pairs
        internal const EventOpcode PerfDiscovered = (EventOpcode)26;
        internal const EventOpcode PerfIssue = (EventOpcode)27;
        internal const EventOpcode IntegrationPoint = (EventOpcode)28;
        internal const EventOpcode IntegrationFailure = (EventOpcode)29;
        internal const EventOpcode ReaderDiscovered = (EventOpcode)30;
        internal const EventOpcode ReaderFailure = (EventOpcode)31;
        internal const EventOpcode DiagnosticInfo = (EventOpcode)32;
        internal const EventOpcode DiagnosticFailure = (EventOpcode)33;
        internal const EventOpcode UINotify = (EventOpcode)34;
        internal const EventOpcode UIWarn = (EventOpcode)35;
        internal const EventOpcode DatabaseEntry = (EventOpcode)36;
        internal const EventOpcode DatabaseFailure = (EventOpcode)37;
        internal const EventOpcode AuditTrail = (EventOpcode)38;
        internal const EventOpcode AuditWarn = (EventOpcode)39;
        internal const EventOpcode NetworkConnection = (EventOpcode)40;
        internal const EventOpcode NetworkFailure = (EventOpcode)41;
        internal const EventOpcode OSInfo = (EventOpcode)42;
        internal const EventOpcode OSFailure = (EventOpcode)43;
        internal const EventOpcode ConfigSetting = (EventOpcode)44;
        internal const EventOpcode ConfigWarn = (EventOpcode)45;
        internal const EventOpcode ConfigFailure = (EventOpcode)46;
        internal const EventOpcode ExceptionFailure = (EventOpcode)47;
        internal const EventOpcode ActivityLoggerFallbackOp = (EventOpcode)48;
        internal const EventOpcode RulesEngineFailure = (EventOpcode)49;
        internal const EventOpcode ActionFailure = (EventOpcode)50;
        internal const EventOpcode AuditCaptureOp = (EventOpcode)51;
        internal const EventOpcode AuditAnalyzeOp = (EventOpcode)52;
        internal const EventOpcode ReadersCaptureOp = (EventOpcode)53;
        internal const EventOpcode ReadersAnalyzeOp = (EventOpcode)54;
        internal const EventOpcode SecurityWarn = (EventOpcode)55;


    }


}
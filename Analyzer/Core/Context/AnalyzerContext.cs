// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  AnalyzerContext.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;
using Microsoft.Extensions.Logging;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Context;



public sealed class AnalyzerContext : IAnalyzerContext
{
    public AnalyzerContext(ILogger logger, ITimeProvider time, ActionLogger actionLogger, IRegistryReader registry,
                           ICimReader cim, IEventLogReader eventLog, IFirewallReader firewall, IEnvReader env)
    {
        Logger = logger;
        Time = time;
        ActionLogger = actionLogger;
        Registry = registry;
        Cim = cim;
        EventLog = eventLog;
        Firewall = firewall;
        Environment = env;
    }





    public ILogger Logger
    {
        get;
    }

    public ITimeProvider Time
    {
        get;
    }

    public ActionLogger ActionLogger
    {
        get;
    }

    public IRegistryReader Registry
    {
        get;
    }

    public ICimReader Cim
    {
        get;
    }

    public IEventLogReader EventLog
    {
        get;
    }

    public IFirewallReader Firewall
    {
        get;
    }

    public IEnvReader Environment
    {
        get;
    }
}
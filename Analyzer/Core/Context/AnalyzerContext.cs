using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Context;

public sealed class AnalyzerContext : IAnalyzerContext
{
    public AnalyzerContext(Microsoft.Extensions.Logging.ILogger logger, ITimeProvider time, ActionLogger actionLogger, IRegistryReader registry, ICimReader cim, IEventLogReader eventLog, IFirewallReader firewall, IEnvReader env)
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

    public Microsoft.Extensions.Logging.ILogger Logger
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

    ITimeProvider IAnalyzerContext.Time => throw new NotImplementedException();

    ActionLogger IAnalyzerContext.ActionLogger => throw new NotImplementedException();
}

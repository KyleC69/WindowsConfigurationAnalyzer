using WindowsConfigurationAnalyzer.Contracts;
using WindowsConfigurationAnalyzer.Infrastructure;

namespace WindowsConfigurationAnalyzer.Context;

internal sealed class AnalyzerContext : IAnalyzerContext
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

 public Microsoft.Extensions.Logging.ILogger Logger { get; }
 public ITimeProvider Time { get; }
 public ActionLogger ActionLogger { get; }
 public IRegistryReader Registry { get; }
 public ICimReader Cim { get; }
 public IEventLogReader EventLog { get; }
 public IFirewallReader Firewall { get; }
 public IEnvReader Environment { get; }
}

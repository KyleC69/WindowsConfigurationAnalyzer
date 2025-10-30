using FluentAssertions;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Engine;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;







namespace KC.WindowsConfigurationAnalyzer.Tests;

public class EngineTests
{
    [Fact]
    public async Task Engine_Runs_Modules_And_Produces_Result()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger("test");
        var actionLogger = new ActionLogger(logger);
        var ctx = new TestContext(logger, new SystemTimeProvider(), actionLogger);
        var engine = new AnalyzerEngine(logger);
        engine.AddModule(new DummyModule());
        var result = await engine.RunAllAsync(ctx);
        result.Should().NotBeNull();
        result.Areas.Should().HaveCount(1);
        result.ActionLog.Should().NotBeEmpty();
    }

    private sealed class DummyModule : IAnalyzerModule
    {
        public string Name => "Dummy";
        public string Area => "Test";
        public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
        => Task.FromResult(new AreaResult(Area, new
        {
            Ok = true
        }, new
        {
            Message = "Hello"
        }, Array.Empty<Finding>(), Array.Empty<string>(), Array.Empty<string>()));
    }

    private sealed class TestContext : IAnalyzerContext
    {
        public TestContext(Microsoft.Extensions.Logging.ILogger logger, ITimeProvider time, ActionLogger actionLogger)
        {
            Logger = logger; Time = time; ActionLogger = actionLogger;
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
        public IRegistryReader Registry => new NullRegistry();
        public ICimReader Cim => new NullCim();
        public IEventLogReader EventLog => new NullEvent();
        public IFirewallReader Firewall => new NullFirewall();
        public IEnvReader Environment => new NullEnv();

        private sealed class NullRegistry : IRegistryReader
        {
            public IEnumerable<string> EnumerateSubKeys(string hiveAndPath) => Array.Empty<string>();
            public IEnumerable<string> EnumerateValueNames(string hiveAndPath) => Array.Empty<string>();
            public object? GetValue(string hiveAndPath, string name) => null;
        }
        private sealed class NullCim : ICimReader
        {
            public IEnumerable<IDictionary<string, object?>> Query(string wql, string? scope = null) => Array.Empty<IDictionary<string, object?>>();
        }
        private sealed class NullEvent : IEventLogReader
        {
            public EventLogSummary? GetSummary(string logName) => null;
        }
        private sealed class NullFirewall : IFirewallReader
        {
            public IEnumerable<object> GetRules() => Array.Empty<object>();
            public IEnumerable<string> GetProfiles() => Array.Empty<string>();
        }
        private sealed class NullEnv : IEnvReader
        {
            public bool Is64BitOS => true;
            public string MachineName => "TEST";
            public string OSVersionString => "TestOS";
            public string UserDomainName => "TESTDOM";
            public string UserName => "tester";
            public IReadOnlyDictionary<string, string?> GetEnvironmentVariables() => new Dictionary<string, string?>();
        }
    }
}

// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Tests
// File:  EngineTests.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;





namespace KC.WindowsConfigurationAnalyzer.Tests;


[TestClass]
public class EngineTests
{


    [TestMethod]
    public async Task Engine_Runs_Modules_And_Produces_Result()
    {
    }





    private sealed class DummyModule : IAnalyzerModule
    {


        public string Name => "Dummy";
        public string Area => "Test";





        public Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(new AreaResult(Area, new
            {
                Ok = true
            }, new
            {
                Message = "Hello"
            }, Array.Empty<Finding>(), Array.Empty<string>(), Array.Empty<string>()));
        }


    }



    private sealed class TestContext : IAnalyzerContext
    {


        public DateTime Time { get; }
        public IActivityLogger? ActionLogger { get; }


        public IRegistryReader Registry => new NullRegistry();
        public ICimReader Cim => new NullCim();
        public IEventLogReader EventLog => new NullEvent();
        public IFirewallReader Firewall => new NullFirewall();
        public IEnvReader Environment => new NullEnv();



        private sealed class NullRegistry : IRegistryReader
        {


            public IEnumerable<string> EnumerateSubKeys(string hiveAndPath)
            {
                return Array.Empty<string>();
            }





            public IEnumerable<string> EnumerateValueNames(string hiveAndPath)
            {
                return Array.Empty<string>();
            }





            public object? GetValue(string hiveAndPath, string name)
            {
                return null;
            }


        }



        private sealed class NullCim : ICimReader
        {


            public Task<IReadOnlyList<IDictionary<string, object?>>> QueryAsync(string wql, string? scope = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<IDictionary<string, object?>>>(Array.Empty<IDictionary<string, object?>>());
            }





            public Task<IReadOnlyList<IDictionary<string, object?>>> QueryAsync(string wql, string? scope = null, CancellationToken cancellationToken = default, string callerName = "", string callerPage = "")
            {
                return Task.FromResult<IReadOnlyList<IDictionary<string, object?>>>(Array.Empty<IDictionary<string, object?>>());
            }


        }



        private sealed class NullEvent : IEventLogReader
        {


            public EventLogSummary? GetSummary(string logName)
            {
                return null;
            }


        }



        private sealed class NullFirewall : IFirewallReader
        {


            public IEnumerable<object> GetRules()
            {
                return Array.Empty<object>();
            }





            public IEnumerable<string> GetProfiles()
            {
                return Array.Empty<string>();
            }


        }



        private sealed class NullEnv : IEnvReader
        {


            public bool Is64BitOS => true;
            public string MachineName => "TEST";
            public string OSVersionString => "TestOS";
            public string UserDomainName => "TESTDOM";
            public string UserName => "tester";





            public IReadOnlyDictionary<string, string?> GetEnvironmentVariables()
            {
                return new Dictionary<string, string?>();
            }


        }


    }


}
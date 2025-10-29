using System.Collections;
using System.Reflection;
using WindowsConfigurationAnalyzer.Contracts;

namespace WindowsConfigurationAnalyzer.Readers;

public sealed class FirewallReader : IFirewallReader
{
 public IEnumerable<string> GetProfiles()
 {
 try
 {
 var policy = CreatePolicy2();
 if (policy is null) return Array.Empty<string>();
 var policyType = policy.GetType();
 var typesObj = policyType.InvokeMember("CurrentProfileTypes", BindingFlags.GetProperty, null, policy, null);
 var types = typesObj is int i ? i :0;
 var list = new List<string>();
 if ((types &0x1) !=0) list.Add("Domain");
 if ((types &0x2) !=0) list.Add("Private");
 if ((types &0x4) !=0) list.Add("Public");
 return list;
 }
 catch
 {
 return Array.Empty<string>();
 }
 }

 public IEnumerable<object> GetRules()
 {
 try
 {
 var policy = CreatePolicy2();
 if (policy is null) return Array.Empty<object>();
 var policyType = policy.GetType();
 var rulesObj = policyType.InvokeMember("Rules", BindingFlags.GetProperty, null, policy, null);
 if (rulesObj is not IEnumerable rules) return Array.Empty<object>();
 var list = new List<object>();
 foreach (var r in rules)
 {
 var t = r!.GetType();
 object? Get(string name)
 {
 try { return t.InvokeMember(name, BindingFlags.GetProperty, null, r, null); }
 catch { return null; }
 }
 list.Add(new
 {
 Name = Get("Name"),
 Enabled = Get("Enabled"),
 Direction = Get("Direction"),
 Action = Get("Action"),
 LocalPorts = Get("LocalPorts"),
 RemotePorts = Get("RemotePorts"),
 LocalAddresses = Get("LocalAddresses"),
 RemoteAddresses = Get("RemoteAddresses"),
 ApplicationName = Get("ApplicationName"),
 Protocol = Get("Protocol")
 });
 }
 return list;
 }
 catch
 {
 return Array.Empty<object>();
 }
 }

 private static object? CreatePolicy2()
 {
 try
 {
 var t = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
 return t is null ? null : Activator.CreateInstance(t);
 }
 catch
 {
 return null;
 }
 }
}

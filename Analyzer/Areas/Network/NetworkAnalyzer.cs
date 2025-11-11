// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  NetworkAnalyzer.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Network;



public sealed class NetworkAnalyzer : IAnalyzerModule
{
    public string Name => "Network Analyzer";
    public string Area => "Network";





    public Task<AreaResult> AnalyzeAsync(IAnalyzerContext context, CancellationToken cancellationToken)
    {
        string area = Area;
        context.ActionLogger.Info(area, "Start", "Collecting network configuration");
        List<string> warnings = new();
        List<string> errors = new();

        // Adapters (legacy WMI)
        List<object> adapters = new();
        try
        {
            context.ActionLogger.Info(area, "Adapters", "Start");
            foreach (var n in context.Cim.Query(
                         "SELECT Index, Description, SettingID, MACAddress, IPEnabled, DHCPEnabled, IPAddress, IPSubnet, DefaultIPGateway, DNSServerSearchOrder, DNSDomainSuffixSearchOrder, DHCPServer, FullDNSRegistrationEnabled, WINSPrimaryServer, WINSSecondaryServer, TcpipNetbiosOptions FROM Win32_NetworkAdapterConfiguration"))
            {
                string[] ips = n.GetAs<string[]>("IPAddress") ?? Array.Empty<string>();
                string[] subnets = n.GetAs<string[]>("IPSubnet") ?? Array.Empty<string>();
                string[] gws = n.GetAs<string[]>("DefaultIPGateway") ?? Array.Empty<string>();
                string[] dns = n.GetAs<string[]>("DNSServerSearchOrder") ?? Array.Empty<string>();
                string[] dnsSuf = n.GetAs<string[]>("DNSDomainSuffixSearchOrder") ?? Array.Empty<string>();
                adapters.Add(new
                {
                    Index = n.GetOrDefault("Index"),
                    Description = n.GetOrDefault("Description"),
                    Guid = n.GetOrDefault("SettingID"),
                    MAC = n.GetOrDefault("MACAddress"),
                    IPEnabled = n.GetOrDefault("IPEnabled"),
                    DHCPEnabled = n.GetOrDefault("DHCPEnabled"),
                    DHCPServer = n.GetOrDefault("DHCPServer"),
                    IPAddresses = ips,
                    Subnets = subnets,
                    Gateways = gws,
                    DnsServers = dns,
                    DnsSuffixes = dnsSuf,
                    FullDNSRegistrationEnabled = n.GetOrDefault("FullDNSRegistrationEnabled"),
                    WINSPrimaryServer = n.GetOrDefault("WINSPrimaryServer"),
                    WINSSecondaryServer = n.GetOrDefault("WINSSecondaryServer"),
                    TcpipNetbiosOptions = n.GetOrDefault("TcpipNetbiosOptions")
                });
            }

            context.ActionLogger.Info(area, "Adapters", $"Complete: count={adapters.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Adapter configuration query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Adapters", "Adapter configuration query failed", ex);
        }

        // Adapter operational (Win32_NetworkAdapter)
        List<object> adaptersOp = new();
        try
        {
            context.ActionLogger.Info(area, "AdaptersOp", "Start");
            foreach (var a in context.Cim.Query(
                         "SELECT Index, Name, NetEnabled, PhysicalAdapter, Speed, NetConnectionStatus, Manufacturer FROM Win32_NetworkAdapter WHERE PhysicalAdapter=TRUE"))
            {
                adaptersOp.Add(new
                {
                    Index = a.GetOrDefault("Index"),
                    Name = a.GetOrDefault("Name"),
                    NetEnabled = a.GetOrDefault("NetEnabled"),
                    PhysicalAdapter = a.GetOrDefault("PhysicalAdapter"),
                    Speed = a.GetOrDefault("Speed"),
                    NetConnectionStatus = a.GetOrDefault("NetConnectionStatus"),
                    Manufacturer = a.GetOrDefault("Manufacturer")
                });
            }

            context.ActionLogger.Info(area, "AdaptersOp", $"Complete: count={adaptersOp.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Adapter operational query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "AdaptersOp", "Adapter operational query failed", ex);
        }

        // Modern IP Interface (StandardCimv2)
        List<object> ipIf = new();
        List<object> ipAddrs = new();
        List<object> routes = new();
        List<object> dnsServerAddrs = new();
        List<object> connProfiles = new();
        try
        {
            context.ActionLogger.Info(area, "IPInterfaces", "Start");
            foreach (var i in context.Cim.Query(
                         "SELECT InterfaceAlias, InterfaceIndex, AddressFamily, Dhcp, DadState, ConnectionState, NlMtu, ManagedAddressConfiguration FROM MSFT_NetIPInterface",
                         "\\\\.\\root\\StandardCimv2"))
            {
                ipIf.Add(new
                {
                    InterfaceAlias = i.GetOrDefault("InterfaceAlias"),
                    InterfaceIndex = i.GetOrDefault("InterfaceIndex"),
                    AddressFamily = i.GetOrDefault("AddressFamily"),
                    Dhcp = i.GetOrDefault("Dhcp"),
                    DadState = i.GetOrDefault("DadState"),
                    ConnectionState = i.GetOrDefault("ConnectionState"),
                    Mtu = i.GetOrDefault("NlMtu"),
                    ManagedAddressConfiguration = i.GetOrDefault("ManagedAddressConfiguration")
                });
            }

            context.ActionLogger.Info(area, "IPInterfaces", $"Complete: count={ipIf.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"MSFT_NetIPInterface query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "IPInterfaces", "MSFT_NetIPInterface query failed", ex);
        }

        try
        {
            context.ActionLogger.Info(area, "IPAddresses", "Start");
            foreach (var a in context.Cim.Query(
                         "SELECT InterfaceAlias, InterfaceIndex, IPAddress, PrefixLength, AddressFamily, Type FROM MSFT_NetIPAddress",
                         "\\\\.\\root\\StandardCimv2"))
            {
                ipAddrs.Add(new
                {
                    InterfaceAlias = a.GetOrDefault("InterfaceAlias"),
                    InterfaceIndex = a.GetOrDefault("InterfaceIndex"),
                    IPAddress = a.GetOrDefault("IPAddress"),
                    PrefixLength = a.GetOrDefault("PrefixLength"),
                    AddressFamily = a.GetOrDefault("AddressFamily"),
                    Type = a.GetOrDefault("Type")
                });
            }

            context.ActionLogger.Info(area, "IPAddresses", $"Complete: count={ipAddrs.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"MSFT_NetIPAddress query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "IPAddresses", "MSFT_NetIPAddress query failed", ex);
        }

        try
        {
            context.ActionLogger.Info(area, "Routes", "Start");
            foreach (var r in context.Cim.Query(
                         "SELECT InterfaceAlias, InterfaceIndex, DestinationPrefix, NextHop, RouteMetric FROM MSFT_NetRoute",
                         "\\\\.\\root\\StandardCimv2"))
            {
                routes.Add(new
                {
                    InterfaceAlias = r.GetOrDefault("InterfaceAlias"),
                    InterfaceIndex = r.GetOrDefault("InterfaceIndex"),
                    DestinationPrefix = r.GetOrDefault("DestinationPrefix"),
                    NextHop = r.GetOrDefault("NextHop"),
                    RouteMetric = r.GetOrDefault("RouteMetric")
                });
            }

            context.ActionLogger.Info(area, "Routes", $"Complete: count={routes.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"MSFT_NetRoute query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Routes", "MSFT_NetRoute query failed", ex);
        }

        try
        {
            context.ActionLogger.Info(area, "DnsServers", "Start");
            foreach (var d in context.Cim.Query(
                         "SELECT InterfaceAlias, InterfaceIndex, AddressFamily, ServerAddresses FROM MSFT_DNSClientServerAddress",
                         "\\\\.\\root\\StandardCimv2"))
            {
                string[] sv = d.GetAs<string[]>("ServerAddresses") ?? Array.Empty<string>();
                dnsServerAddrs.Add(new
                {
                    InterfaceAlias = d.GetOrDefault("InterfaceAlias"),
                    InterfaceIndex = d.GetOrDefault("InterfaceIndex"),
                    AddressFamily = d.GetOrDefault("AddressFamily"),
                    ServerAddresses = sv
                });
            }

            context.ActionLogger.Info(area, "DnsServers", $"Complete: ifaces={dnsServerAddrs.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"MSFT_DNSClientServerAddress query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "DnsServers", "MSFT_DNSClientServerAddress query failed", ex);
        }

        try
        {
            context.ActionLogger.Info(area, "Profiles", "Start");
            foreach (var p in context.Cim.Query(
                         "SELECT Name, InterfaceAlias, NetworkCategory FROM MSFT_NetConnectionProfile",
                         "\\\\.\\root\\StandardCimv2"))
            {
                connProfiles.Add(new
                {
                    Name = p.GetOrDefault("Name"),
                    InterfaceAlias = p.GetOrDefault("InterfaceAlias"),
                    NetworkCategory = p.GetOrDefault("NetworkCategory")
                });
            }

            context.ActionLogger.Info(area, "Profiles", $"Complete: count={connProfiles.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"MSFT_NetConnectionProfile query failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Profiles", "MSFT_NetConnectionProfile query failed", ex);
        }

        // Firewall via reader (existing integration)
        string[] firewallProfiles = Array.Empty<string>();
        object[] firewallRules = Array.Empty<object>();
        try
        {
            context.ActionLogger.Info(area, "Firewall", "Start");
            firewallProfiles = context.Firewall.GetProfiles().ToArray();
            firewallRules = context.Firewall.GetRules().ToArray();
            context.ActionLogger.Info(area, "Firewall",
                $"Complete: profiles={firewallProfiles.Length}, rules={firewallRules.Length}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Firewall reader failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Firewall", "Firewall reader failed", ex);
        }

        // TLS/Schannel protocols (serverside/client)
        List<object> tls = new();
        try
        {
            context.ActionLogger.Info(area, "TLS", "Start");
            string protoRoot = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\SCHANNEL\\Protocols";
            foreach (string proto in context.Registry.EnumerateSubKeys(protoRoot))
            {
                foreach (string role in new[] { "Server", "Client" })
                {
                    string k = $"{protoRoot}\\{proto}\\{role}";
                    int? enabled = null, disabledByDefault = null;
                    try
                    {
                        enabled = context.Registry.GetValue(k, "Enabled") as int?;
                    }
                    catch
                    {
                    }

                    try
                    {
                        disabledByDefault = context.Registry.GetValue(k, "DisabledByDefault") as int?;
                    }
                    catch
                    {
                    }

                    tls.Add(new
                    {
                        Protocol = proto,
                        Role = role,
                        Enabled = enabled,
                        DisabledByDefault = disabledByDefault
                    });
                }
            }

            context.ActionLogger.Info(area, "TLS", $"Complete: entries={tls.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"TLS/Schannel read failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "TLS", "TLS/Schannel read failed", ex);
        }

        // TCP Global Parameters
        Dictionary<string, object?> tcpParams = new();
        try
        {
            context.ActionLogger.Info(area, "TcpParams", "Start");
            string baseKey = "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters";
            foreach (string name in new[]
                     {
                         "TcpTimedWaitDelay", "MaxUserPort", "TcpNumConnections", "EnablePMTUDiscovery",
                         "EnablePMTUBHDetect", "DeadGWDetectDefault", "DisableTaskOffload"
                     })
            {
                try
                {
                    tcpParams[name] = context.Registry.GetValue(baseKey, name) ?? string.Empty;
                }
                catch
                {
                }
            }

            context.ActionLogger.Info(area, "TcpParams", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"TCP parameters read failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "TcpParams", "TCP parameters read failed", ex);
        }

        // Wi-Fi profiles (registry best-effort)
        List<object> wifiProfiles = new();
        try
        {
            context.ActionLogger.Info(area, "Wifi", "Start");
            string ifRoot = "HKLM\\SOFTWARE\\Microsoft\\Wlansvc\\Profiles\\Interfaces";
            foreach (string iface in context.Registry.EnumerateSubKeys(ifRoot))
            {
                string ifaceKey = $"{ifRoot}\\{iface}";
                foreach (string prof in context.Registry.EnumerateSubKeys(ifaceKey))
                {
                    string key = $"{ifaceKey}\\{prof}";
                    string? name = context.Registry.GetValue(key, "Name")?.ToString();
                    wifiProfiles.Add(new { Interface = iface, Profile = prof, Name = name });
                }
            }

            context.ActionLogger.Info(area, "Wifi", $"Complete: profiles={wifiProfiles.Count}");
        }
        catch (Exception ex)
        {
            warnings.Add($"Wi-Fi profiles read failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Wifi", "Wi-Fi profiles read failed", ex);
        }

        // Hosts file metadata
        Dictionary<string, object?> hostsMeta = new();
        try
        {
            context.ActionLogger.Info(area, "Hosts", "Start");
            string win = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string hosts = Path.Combine(win, "System32", "drivers", "etc", "hosts");
            if (File.Exists(hosts))
            {
                FileInfo fi = new(hosts);
                hostsMeta["Path"] = hosts;
                hostsMeta["Size"] = fi.Length;
                hostsMeta["LastWriteUtc"] = fi.LastWriteTimeUtc;
            }

            context.ActionLogger.Info(area, "Hosts", "Complete");
        }
        catch (Exception ex)
        {
            warnings.Add($"Hosts file inspection failed: {ex.Message}");
            errors.Add(ex.ToString());
            context.ActionLogger.Error(area, "Hosts", "Hosts file inspection failed", ex);
        }

        // Summaries
        string[] activeProfiles = firewallProfiles;
        int ipv4Count = ipAddrs.Count(a => (a as dynamic).AddressFamily?.ToString() == "2");
        int ipv6Count = ipAddrs.Count(a => (a as dynamic).AddressFamily?.ToString() == "23");
        int defaultRoutes = routes.Count(r => (r as dynamic).DestinationPrefix?.ToString() is "0.0.0.0/0" or "::/0");
        var summary = new
        {
            Adapters = adapters.Count,
            IPv4Addresses = ipv4Count,
            IPv6Addresses = ipv6Count,
            Routes = routes.Count,
            DefaultRoutes = defaultRoutes,
            FirewallProfiles = activeProfiles
        };
        var details = new
        {
            Adapters = adapters,
            AdaptersOperational = adaptersOp,
            IPInterfaces = ipIf,
            IPAddresses = ipAddrs,
            Routes = routes,
            DNSClientServerAddresses = dnsServerAddrs,
            ConnectionProfiles = connProfiles,
            FirewallProfiles = activeProfiles,
            FirewallRules = firewallRules,
            TLS = tls,
            TcpParameters = tcpParams,
            WifiProfiles = wifiProfiles,
            Hosts = hostsMeta
        };
        AreaResult result = new(area, summary, details, Array.Empty<Finding>(), warnings, errors);
        context.ActionLogger.Info(area, "Complete", "Network configuration collected");

        return Task.FromResult(result);
    }
}
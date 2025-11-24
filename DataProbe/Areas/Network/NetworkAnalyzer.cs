//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   NetworkAnalyzer.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Network;


public sealed class NetworkAnalyzer : IAnalyzerModule
{


    private IActivityLogger? _logger;


    public string Name => "Network Analyzer";
    public string Area => "Network";





    public async Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken)
    {
        _logger = logger;
        string area = Area;
        _logger.Log(area, "Start", "Collecting network configuration");
        List<string> warnings = [];
        List<string> errors = [];

        // Adapters
        List<object> adapters = [];
        try
        {
            _logger.Log(area, "Adapters", "Start");
            IReadOnlyList<IDictionary<string, object?>> adapterCfg = await context.Cim.QueryAsync(
                "SELECT Index, Description, SettingID, MACAddress, IPEnabled, DHCPEnabled, IPAddress, IPSubnet, DefaultIPGateway, DNSServerSearchOrder, DNSDomainSuffixSearchOrder, DHCPServer, FullDNSRegistrationEnabled, WINSPrimaryServer, WINSSecondaryServer, TcpipNetbiosOptions FROM Win32_NetworkAdapterConfiguration",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> n in adapterCfg)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string[] ips = n.GetAs<string[]>("IPAddress") ?? [];
                string[] subnets = n.GetAs<string[]>("IPSubnet") ?? [];
                string[] gws = n.GetAs<string[]>("DefaultIPGateway") ?? [];
                string[] dns = n.GetAs<string[]>("DNSServerSearchOrder") ?? [];
                string[] dnsSuf = n.GetAs<string[]>("DNSDomainSuffixSearchOrder") ?? [];
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

            _logger.Log("INF", $"Complete: count={adapters.Count}", "NetworkAnalyzer - Adapters");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Adapter configuration query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Adapter configuration query failed: {ex.Message}", "NetworkAnalyzer - Adapters");
        }

        // Adapter operational
        List<object> adaptersOp = [];
        try
        {
            _logger.Log("INF", "Starting search for adapter details", "NetworkAnalyzer - AdaptersOp");
            IReadOnlyList<IDictionary<string, object?>> op = await context.Cim.QueryAsync(
                "SELECT Index, Name, NetEnabled, PhysicalAdapter, Speed, NetConnectionStatus, Manufacturer FROM Win32_NetworkAdapter WHERE PhysicalAdapter=TRUE",
                null, cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> a in op)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            _logger.Log("INF", $"Complete: count={adaptersOp.Count}", "NetworkAnalyzer - AdaptersOp");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Adapter operational query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Adapter operational query failed: {ex.Message}", "NetworkAnalyzer - AdaptersOp");
        }

        // Modern IP Interface (StandardCimv2)
        List<object> ipIf = [];
        List<object> ipAddrs = [];
        List<object> routes = [];
        List<object> dnsServerAddrs = [];
        List<object> connProfiles = [];
        try
        {
            _logger.Log("INF", "Starting search for IP interfaces", "NetworkAnalyzer - IPInterfaces");
            IReadOnlyList<IDictionary<string, object?>> ifs = await context.Cim.QueryAsync(
                "SELECT InterfaceAlias, InterfaceIndex, AddressFamily, Dhcp, DadState, ConnectionState, NlMtu, ManagedAddressConfiguration FROM MSFT_NetIPInterface",
                "\\\\.\\root\\StandardCimv2", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> i in ifs)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            _logger.Log("INF", $"Complete: count={ipIf.Count}", "NetworkAnalyzer - IPInterfaces");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"MSFT_NetIPInterface query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"MSFT_NetIPInterface query failed: {ex.Message}", "NetworkAnalyzer - IPInterfaces");
        }

        try
        {
            _logger.Log("INF", "Starting search for IP addresses", "NetworkAnalyzer - IPAddresses");
            IReadOnlyList<IDictionary<string, object?>> addrs = await context.Cim.QueryAsync(
                "SELECT InterfaceAlias, InterfaceIndex, IPAddress, PrefixLength, AddressFamily, Type FROM MSFT_NetIPAddress",
                "\\\\.\\root\\StandardCimv2", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> a in addrs)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

            _logger.Log("INF", $"Complete: count={ipAddrs.Count}", "NetworkAnalyzer - IPAddresses");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"MSFT_NetIPAddress query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"MSFT_NetIPAddress query failed: {ex.Message}", "NetworkAnalyzer - IPAddresses");
        }

        try
        {
            _logger.Log("INF", "Starting search for routes", "NetworkAnalyzer - Routes");
            IReadOnlyList<IDictionary<string, object?>> rts = await context.Cim.QueryAsync(
                "SELECT InterfaceAlias, InterfaceIndex, DestinationPrefix, NextHop, RouteMetric FROM MSFT_NetRoute",
                "\\\\.\\root\\StandardCimv2", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> r in rts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                routes.Add(new
                {
                    InterfaceAlias = r.GetOrDefault("InterfaceAlias"),
                    InterfaceIndex = r.GetOrDefault("InterfaceIndex"),
                    DestinationPrefix = r.GetOrDefault("DestinationPrefix"),
                    NextHop = r.GetOrDefault("NextHop"),
                    RouteMetric = r.GetOrDefault("RouteMetric")
                });
            }

            _logger.Log("INF", $"Complete: count={routes.Count}", "NetworkAnalyzer - Routes");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"MSFT_NetRoute query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"MSFT_NetRoute query failed: {ex.Message}", "NetworkAnalyzer - Routes");
        }

        try
        {
            _logger.Log("INF", "Starting search for DNS servers", "NetworkAnalyzer - DnsServers");
            IReadOnlyList<IDictionary<string, object?>> dn = await context.Cim.QueryAsync(
                "SELECT InterfaceAlias, InterfaceIndex, AddressFamily, ServerAddresses FROM MSFT_DNSClientServerAddress",
                "\\\\.\\root\\StandardCimv2", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> d in dn)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string[] sv = d.GetAs<string[]>("ServerAddresses") ?? [];
                dnsServerAddrs.Add(new
                {
                    InterfaceAlias = d.GetOrDefault("InterfaceAlias"),
                    InterfaceIndex = d.GetOrDefault("InterfaceIndex"),
                    AddressFamily = d.GetOrDefault("AddressFamily"),
                    ServerAddresses = sv
                });
            }

            _logger.Log("INF", $"Complete: ifaces={dnsServerAddrs.Count}", "NetworkAnalyzer - DnsServers");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"MSFT_DNSClientServerAddress query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"MSFT_DNSClientServerAddress query failed: {ex.Message}", "NetworkAnalyzer - DnsServers");
        }

        try
        {
            _logger.Log("INF", "Starting search for connection profiles", "NetworkAnalyzer - Profiles");
            IReadOnlyList<IDictionary<string, object?>> profs = await context.Cim.QueryAsync(
                "SELECT Name, InterfaceAlias, NetworkCategory FROM MSFT_NetConnectionProfile",
                "\\\\.\\root\\StandardCimv2", cancellationToken).ConfigureAwait(false);
            foreach (IDictionary<string, object?> p in profs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                connProfiles.Add(new
                {
                    Name = p.GetOrDefault("Name"),
                    InterfaceAlias = p.GetOrDefault("InterfaceAlias"),
                    NetworkCategory = p.GetOrDefault("NetworkCategory")
                });
            }

            _logger.Log("INF", $"Complete: count={connProfiles.Count}", "NetworkAnalyzer - Profiles");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"MSFT_NetConnectionProfile query failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"MSFT_NetConnectionProfile query failed: {ex.Message}", "NetworkAnalyzer - Profiles");
        }

        // Firewall
        string[] firewallProfiles = [];
        object[] firewallRules = [];
        try
        {
            _logger.Log("INF", "Starting search for firewall profiles", "NetworkAnalyzer - Firewall");
            firewallProfiles = context.Firewall.GetProfiles().ToArray();
            firewallRules = context.Firewall.GetRules().ToArray();
            _logger.Log("INF", $"Complete: profiles={firewallProfiles.Length}, rules={firewallRules.Length}", "NetworkAnalyzer - Firewall");
        }
        catch (Exception ex)
        {
            warnings.Add($"Firewall reader failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Firewall reader failed: {ex.Message}", "NetworkAnalyzer - Firewall");
        }

        // TLS/Schannel
        List<object> tls = [];
        try
        {
            _logger.Log("INF", "Starting search for TLS protocols", "NetworkAnalyzer - TLS");
            string protoRoot = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\SecurityProviders\\SCHANNEL\\Protocols";
            foreach (string proto in context.Registry.EnumerateSubKeys(protoRoot))
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (string? role in new[] { "Server", "Client" })
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

                    tls.Add(new { Protocol = proto, Role = role, Enabled = enabled, DisabledByDefault = disabledByDefault });
                }
            }

            _logger.Log("INF", $"Complete: entries={tls.Count}", "NetworkAnalyzer - TLS");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"TLS/Schannel read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"TLS/Schannel read failed: {ex.Message}", "NetworkAnalyzer - TLS");
        }

        // TCP Global Parameters
        Dictionary<string, object?> tcpParams = [];
        try
        {
            _logger.Log("INF", "Starting search for TCP global parameters", "NetworkAnalyzer - TcpParams");
            string baseKey = "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters";
            foreach (string? name in new[] { "TcpTimedWaitDelay", "MaxUserPort", "TcpNumConnections", "EnablePMTUDiscovery", "EnablePMTUBHDetect", "DeadGWDetectDefault", "DisableTaskOffload" })
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    tcpParams[name] = context.Registry.GetValue(baseKey, name) ?? string.Empty;
                }
                catch
                {
                }
            }

            _logger.Log("INF", "Complete", "NetworkAnalyzer - TcpParams");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"TCP parameters read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"TCP parameters read failed: {ex.Message}", "NetworkAnalyzer - TcpParams");
        }

        // Wi-Fi profiles
        List<object> wifiProfiles = [];
        try
        {
            _logger.Log("INF", "Starting search for Wi-Fi profiles", "NetworkAnalyzer - Wifi  ");
            string ifRoot = "HKLM\\SOFTWARE\\Microsoft\\Wlansvc\\Profiles\\Interfaces";
            foreach (string iface in context.Registry.EnumerateSubKeys(ifRoot))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string ifaceKey = $"{ifRoot}\\{iface}";
                foreach (string prof in context.Registry.EnumerateSubKeys(ifaceKey))
                {
                    string key = $"{ifaceKey}\\{prof}";
                    string? name = context.Registry.GetValue(key, "Name")?.ToString();
                    wifiProfiles.Add(new { Interface = iface, Profile = prof, Name = name });
                }
            }

            _logger.Log("INF", $"Complete: profiles={wifiProfiles.Count}", "NetworkAnalyzer - Wifi");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            warnings.Add($"Wi-Fi profiles read failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Wi-Fi profiles read failed: {ex.Message}", "NetworkAnalyzer - Wifi");
        }

        // Hosts file metadata
        Dictionary<string, object?> hostsMeta = [];
        try
        {
            _logger.Log("INF", "Starting search for Hosts file", "NetworkAnalyzer - Hosts");
            string win = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string hosts = Path.Combine(win, "System32", "drivers", "etc", "hosts");
            if (File.Exists(hosts))
            {
                FileInfo fi = new(hosts);
                hostsMeta["Path"] = hosts;
                hostsMeta["Size"] = fi.Length;
                hostsMeta["LastWriteUtc"] = fi.LastWriteTimeUtc;
            }

            _logger.Log("INF", "Complete", "NetworkAnalyzer - Hosts");
        }
        catch (Exception ex)
        {
            warnings.Add($"Hosts file inspection failed: {ex.Message}");
            errors.Add(ex.ToString());
            _logger.Log("ERR", $"Hosts file inspection failed: {ex.Message}", "NetworkAnalyzer - Hosts");
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
      //  AreaResult result = new(area, summary, details, new List<Finding>().AsReadOnly(), warnings, errors);
        _logger.Log("INF", "Complete", "Network configuration collected");

        return default;
    }


}
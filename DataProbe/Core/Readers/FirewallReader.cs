//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   FirewallReader.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Collections;
using System.Reflection;

using KC.WindowsConfigurationAnalyzer.Contracts;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public sealed class FirewallReader : IFirewallReader
{


    public IEnumerable<string> GetProfiles()
    {
        try
        {
            object? policy = CreatePolicy2();

            if (policy is null)
            {
                return new List<string>().AsReadOnly();
            }

            Type policyType = policy.GetType();
            object? typesObj =
                policyType.InvokeMember("CurrentProfileTypes", BindingFlags.GetProperty, null, policy, null);
            int types = typesObj is int i ? i : 0;
            List<string> list = [];
            if ((types & 0x1) != 0)
            {
                list.Add("Domain");
            }

            if ((types & 0x2) != 0)
            {
                list.Add("Private");
            }

            if ((types & 0x4) != 0)
            {
                list.Add("Public");
            }

            return list;
        }
        catch
        {
            return new List<string>().AsReadOnly();
        }
    }





    public IEnumerable<object> GetRules()
    {
        try
        {
            object? policy = CreatePolicy2();

            if (policy is null)
            {
                return new List<object>().AsReadOnly();
            }

            Type policyType = policy.GetType();
            object? rulesObj = policyType.InvokeMember("Rules", BindingFlags.GetProperty, null, policy, null);

            if (rulesObj is not IEnumerable rules)
            {
                return new List<object>().AsReadOnly();
            }

            List<object> list = [];
            foreach (object? r in rules)
            {
                Type t = r!.GetType();


                object? Get(string name)
                {
                    try
                    {
                        return t.InvokeMember(name, BindingFlags.GetProperty, null, r, null);
                    }
                    catch
                    {
                        return null;
                    }
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
            return new List<object>().AsReadOnly();
        }
    }





    private static object? CreatePolicy2()
    {
        try
        {
            Type? t = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");

            return t is null ? null : Activator.CreateInstance(t);
        }
        catch
        {
            return null;
        }
    }


}
//  Created:  2025/11/23
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   AnalyzerContext.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




// Created:  2025/11/12
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  AnalyzerContext.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




namespace KC.WindowsConfigurationAnalyzer.Contracts;


public sealed class AnalyzerContext : IAnalyzerContext
{


    public AnalyzerContext(IActivityLogger actionLogger, IRegistryReader registry,
        ICimReader cim, IEventLogReader eventLog, IFirewallReader firewall, IEnvReader env)
    {
        Time = DateTime.Now;
        ActionLogger = actionLogger;
        Registry = registry;
        Cim = cim;
        EventLog = eventLog;
        Firewall = firewall;
        Environment = env;
    }






    public DateTime Time { get; }

    public IActivityLogger ActionLogger { get; }

    public IRegistryReader Registry { get; }

    public ICimReader Cim { get; }

    public IEventLogReader EventLog { get; }

    public IFirewallReader Firewall { get; }

    public IEnvReader Environment { get; }


}
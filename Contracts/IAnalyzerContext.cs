//  Created:  2025/11/24
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   IAnalyzerContext.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IAnalyzerContext
{


    DateTime Time { get; }
    IActivityLogger ActionLogger { get; }
    IRegistryReader Registry { get; }
    ICimReader Cim { get; }
    IEventLogReader EventLog { get; }
    IFirewallReader Firewall { get; }
    IEnvReader Environment { get; }


}
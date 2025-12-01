//  Created:  2025/11/24
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   ProbeTarget.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts;


public class ProbeTarget
{


    public SubsystemType Subsystem { get; set; }
    public string Location { get; set; } = string.Empty; // registry path, file path, ETW provider, etc.
    public string Pattern { get; set; } = string.Empty; // optional regex or wildcard


}
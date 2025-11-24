//  Created:  2025/11/23
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   RuleResult.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.

namespace KC.WindowsConfigurationAnalyzer.Contracts;


public class RuleResult
{


    public string RuleName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int SeverityScore { get; set; }
    public string SchemaVersion { get; set; } = string.Empty;


}
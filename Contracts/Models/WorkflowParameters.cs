//  Created:  2025/11/17
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   WorkflowParameters.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;


public class WorkflowParameters
{


    // Shared context values
    public string ProviderName { get; set; } = string.Empty;
    public string ManifestPath { get; set; } = string.Empty;
    public string[] RegistryKeys { get; set; } = Array.Empty<string>();
    public string[] RegisteredProviders { get; set; } = Array.Empty<string>();
    public string[] ChannelNamesFromManifest { get; set; } = Array.Empty<string>();
    public string[] ChannelNamesFromRegistry { get; set; } = Array.Empty<string>();
    public string FileAcl { get; set; } = string.Empty;
    public string RegistryAcls { get; set; } = string.Empty;
    public string WevtutilQuery { get; set; } = string.Empty;
    public DateTime ProbeTimestamp { get; set; }


}



public class WorkflowConstraints
{


    public bool RunSequentially { get; set; } = true;
    public bool StopOnFailure { get; set; } = false;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);


}



public class WorkflowResult
{


    public string RuleName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int SeverityScore { get; set; }
    public string SchemaVersion { get; set; } = string.Empty;


}
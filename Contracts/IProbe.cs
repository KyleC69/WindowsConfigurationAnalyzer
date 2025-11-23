//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   IProbe.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Runtime.CompilerServices;

#endregion





namespace KC.WindowsConfigurationAnalyzer.Contracts;


/// <summary>
///     Contract for all probes (Registry, WMI, FileSystem, ACL, EventLog, etc.)
/// </summary>
public interface IProbe
{


    /// <summary>
    ///     Unique provider name (e.g. "Registry", "WMI", "FileSystem").
    ///     Used to match against Rule.Provider in the workflow.
    /// </summary>
    string Provider { get; }





    /// <summary>
    ///     Execute the probe with the given parameters.
    /// </summary>
    /// <param name="parameters">Provider-specific parameters from the rule JSON.</param>
    /// <param name="token"></param>
    /// <param name="callerName"></param>
    /// <param name="callerFilePath"></param>
    /// <returns>ProbeResult containing the raw value and provenance.</returns>
    Task<ProbeResult> ExecuteAsync(IDictionary<string, object> parameters, CancellationToken token, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "");


}
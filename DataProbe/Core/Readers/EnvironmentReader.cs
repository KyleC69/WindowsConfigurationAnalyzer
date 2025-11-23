//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   EnvironmentReader.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Collections;

using KC.WindowsConfigurationAnalyzer.Contracts;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public sealed class EnvironmentReader : IProbe
{


    private readonly IActivityLogger _logger;





    public EnvironmentReader(IActivityLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }





    public string MachineName => Environment.MachineName;
    public string OSVersionString => Environment.OSVersion.VersionString;
    public bool Is64BitOS => Environment.Is64BitOperatingSystem;
    public string UserName => Environment.UserName;
    public string UserDomainName => Environment.UserDomainName;





    /// <summary>
    ///     Unique provider name (e.g. "Registry", "WMI", "FileSystem").
    ///     Used to match against Rule.Provider in the workflow.
    /// </summary>
    public string Provider => "Environment";





    /// <summary>
    ///     Execute the probe with the given parameters.
    /// </summary>
    /// <param name="parameters">Provider-specific parameters from the rule JSON.</param>
    /// <param name="token"></param>
    /// <param name="callerName"></param>
    /// <param name="callerFilePath"></param>
    /// <returns>ProbeResult containing the raw value and provenance.</returns>
    public async Task<ProbeResult> ExecuteAsync(IDictionary<string, object> parameters, CancellationToken token, string callerName = "", string callerFilePath = "")
    {
        ProbeResult result = new()
        {
            Provider = Provider,
            Metadata = new Dictionary<string, object>
            {
                { "Timestamp", DateTime.UtcNow },
                { "CallerName", callerName },
                { "CallerFilePath", callerFilePath }
            }
        };


        //TODO: Support parameters to get specific environment variables or info


        return result;
    }





    public IReadOnlyDictionary<string, string?> GetEnvironmentVariables()
    {
        Dictionary<string, string?> dict = [];
        foreach (DictionaryEntry kvp in Environment.GetEnvironmentVariables())
        {
            string key = kvp.Key?.ToString() ?? string.Empty;
            string? val = kvp.Value?.ToString();
            if (!dict.ContainsKey(key))
            {
                dict[key] = val;
            }
        }

        return dict;
    }


}
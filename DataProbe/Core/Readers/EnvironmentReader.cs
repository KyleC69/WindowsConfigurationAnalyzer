//  Created:  2025/11/22
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




using KC.WindowsConfigurationAnalyzer.Contracts;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public sealed class EnvironmentReader : IProbe
{


    private readonly IActivityLogger _logger;





    public EnvironmentReader(IActivityLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }





    public string MachineName
    {
        get => Environment.MachineName;
    }


    public string OSVersionString
    {
        get => Environment.OSVersion.VersionString;
    }


    public bool Is64BitOS
    {
        get => Environment.Is64BitOperatingSystem;
    }


    public string UserName
    {
        get => Environment.UserName;
    }


    public string UserDomainName
    {
        get => Environment.UserDomainName;
    }





    /// <summary>
    ///     Unique provider name (e.g. "Registry", "WMI", "FileSystem").
    ///     Used to match against Rule.Provider in the workflow.
    /// </summary>
    public string Provider
    {
        get => "Environment";
    }





    /// <summary>
    ///     Execute the probe with the given parameters.
    /// </summary>
    /// <param name="parameters">Provider-specific parameters from the rule JSON.</param>
    /// <param name="token"></param>
    /// <param name="callerName"></param>
    /// <param name="callerFilePath"></param>
    /// <returns>ProbeResult containing the raw value and provenance.</returns>
    public async Task<ProbeResult> ExecuteAsync(IProviderParameters parameters, CancellationToken token)
    {
        ProbeResult result = new()
        {
            Provider = Provider,
            Metadata = new Dictionary<string, object>
            {
                { "Timestamp", DateTime.UtcNow }
            }
        };
        //  var parms = parameters as EnvironmentParameters;

        //TODO: Support parameters to get specific environment variables or info
        throw new NotImplementedException("EnvironmentReader currently does not support parameters.");
    }


}
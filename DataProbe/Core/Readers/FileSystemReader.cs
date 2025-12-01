//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   FileSystemReader.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using KC.WindowsConfigurationAnalyzer.Contracts;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public class FileSystemReader : IProbe
{


    private readonly IActivityLogger _logger;





    public FileSystemReader(IActivityLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }





    /// <summary>
    ///     Unique provider name (e.g. "Registry", "WMI", "FileSystem").
    ///     Used to match against Rule.Provider in the workflow.
    /// </summary>
    public string Provider
    {
        get => "FileSystem";
    }





    /// <summary>
    ///     Execute the probe with the given parameters.
    /// </summary>
    /// <param name="parameters">Provider-specific parameters from the rule JSON.</param>
    /// <param name="token"></param>
    /// <returns>ProbeResult containing the raw value and provenance.</returns>
    public async Task<ProbeResult> ExecuteAsync(IProviderParameters parameters, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var parm = parameters as FileSystemParameters;
        // Check if System environment variables exist in path
        if (parm!.Path != null && parm!.Path!.Contains("%")) parm.Path = Environment.ExpandEnvironmentVariables(parm.Path);


        ProbeResult result = new()
        {
            Provider = Provider,
            Timestamp = DateTime.UtcNow,
            Metadata = []
        };
        result.Metadata["path"] = parm?.Path ?? string.Empty;


        if (string.IsNullOrEmpty(parm?.Path))
        {
            result.ProbeSuccess = false;
            result.Message = "The 'path' parameter is missing or empty.";
            _logger.Log("ERROR", result.Message, "FileSystemReader");

            return await Task.FromResult(result);
        }

        try
        {
            if (Directory.Exists(parm?.Path))
            {
                result.Value = "Directory exists";
                result.ProbeSuccess = true;
            }
            else if (File.Exists(parm?.Path))
            {
                result.Value = "File exists";
                result.ProbeSuccess = true;
            }
            else
            {
                result.ProbeSuccess = false;
                result.Message = $"The specified path does not exist: {parm?.Path}";
                _logger.Log("WARNING", result.Message, "FileSystemReader");
            }
        }
        catch (Exception ex)
        {
            result.ProbeSuccess = false;
            result.Message = $"An error occurred while checking the path '{parm?.Path}': {ex.Message}";
            _logger.Log("ERROR", result.Message, "FileSystemReader");
        }



        return await Task.FromResult(result);
    }


}
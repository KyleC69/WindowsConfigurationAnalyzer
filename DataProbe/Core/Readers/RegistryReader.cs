//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   RegistryReader.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts;

using Microsoft.Win32;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public sealed class RegistryReader : IProbe
{


    private readonly IActivityLogger _logger;





    public RegistryReader(IActivityLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }





    /// <summary>
    ///     Unique provider name (e.g. "Registry", "WMI", "FileSystem").
    ///     Used to match against Rule.Provider in the workflow.
    /// </summary>
    public string Provider => "Registry";





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
        _logger.Log("INF", $"RegistryReader: Executing probe for caller {callerName} in file {callerFilePath}", "RegReader");
        ProbeResult res = new()
        {
            Provider = Provider,
            Timestamp = DateTime.UtcNow
        };
        try
        {
            await GetValueAsync(parameters, res, token);
        }
        catch (Exception ex)
        {
            _logger.Log("ERR", $"RegistryReader: Error executing probe for caller {callerName} in file {callerFilePath}: {ex.Message}", "RegReader");
        }

        return res;
    }





    private object? GetValue(string hiveAndPath, string name)
    {
        using RegistryKey? key = OpenSubKey(hiveAndPath);

        return key?.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
    }





    public IEnumerable<string> EnumerateSubKeys(string hiveAndPath)
    {
        using RegistryKey? key = OpenSubKey(hiveAndPath);

        return key?.GetSubKeyNames() ?? [];
    }





    public IEnumerable<string> EnumerateValueNames(string hiveAndPath)
    {
        using RegistryKey? key = OpenSubKey(hiveAndPath);

        return key?.GetValueNames() ?? [];
    }





    private static RegistryKey? OpenSubKey(string hiveAndPath)
    {
        (string? hive, string? path) = SplitHive(hiveAndPath);
        RegistryKey? baseKey = hive switch
        {
            "HKLM" or "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
            "HKCU" or "HKEY_CURRENT_USER" => Registry.CurrentUser,
            "HKCR" or "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
            "HKU" or "HKEY_USERS" => Registry.Users,
            "HKCC" or "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
            _ => null
        };

        return baseKey?.OpenSubKey(path, false);
    }





    private static (string hive, string path) SplitHive(string hiveAndPath)
    {
        int idx = hiveAndPath.IndexOf('\\');

        if (idx < 0)
        {
            return (hiveAndPath, string.Empty);
        }

        return (hiveAndPath.Substring(0, idx), hiveAndPath[(idx + 1)..]);
    }





    private async Task<ProbeResult> GetValueAsync(IDictionary<string, object> parameters, ProbeResult res, CancellationToken token)
    {
        string? hiveAndPath = parameters["hiveAndPath"]?.ToString();
        string? name = parameters["name"]?.ToString();

        if (hiveAndPath == null || name == null)
        {
            _logger.Log("ERR", "RegistryReader: Missing parameters for GetValueAsync", "RegReader");
            res.Message = "Missing required parameters 'hiveAndPath' or 'name'.";

            return res;
        }

        try
        {
            object? value = GetValue(hiveAndPath, name);
            res.Value = value;
        }
        catch (Exception e)
        {
            res.Message = e.Message;
        }

        return res;
    }


}
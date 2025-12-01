//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   RegSampleProbe.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using KC.WindowsConfigurationAnalyzer.Contracts;

using Microsoft.Win32;





namespace KC.WindowsConfigurationAnalyzer.DataProbe;


public class RegistryProbe : IProbe
{


    public string Provider
    {
        get => "Registry";
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
        ProbeResult result = new() { Provider = Provider };
        var p = parameters as RegistryParameters;
        try
        {
            var keyPath = p?.Hive + "\\" + p?.Path!;
            var valueName = p?.Key!;

            var key = Registry.GetValue(keyPath, valueName, null);
            result.Value = key;
            result.ProbeSuccess = key != null;
            result.Message = key != null ? "Value retrieved" : "Value not found";
        }
        catch (Exception ex)
        {
            result.ProbeSuccess = false;
            result.Message = $"Registry probe error: {ex.Message}";
        }

        return result;
    }


}
/*
 *
 *
 *
 *
 *
 *
 *
 * 📌 Recommendations
   - Keep probes pluggable: Register them in a dictionary keyed by Provider.
   - Normalize values: Always return object? Value but normalize types (string, int, bool) so condition evaluation is consistent.
   - Condition evaluator: Build a small evaluator class that takes (ProbeResult.Value, Rule.Condition.Operator, Rule.Condition.Expected) and returns true/false.
   - Provenance: Always populate ProbeResult.Metadata with details (path, property, query) for audit clarity.

 *
 *
 *
 *
 *
 *
 *
 *
 */
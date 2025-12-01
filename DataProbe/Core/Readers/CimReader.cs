//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   CimReader.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.Diagnostics;
using System.Text.RegularExpressions;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.PowerShell;

using Microsoft.Management.Infrastructure;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public sealed class CimReader : IProbe
{


    private static readonly TraceSource _trace = new("KC.WindowsConfigurationAnalyzer.Analyzer.Core.Readers.CimReader");
    private static IActivityLogger? _logger;





    public CimReader(IActivityLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }






    /// <summary>
    ///     Unique provider name (e.g. "Registry", "WMI", "FileSystem").
    ///     Used to match against Rule.Provider in the workflow.
    /// </summary>
    public string Provider
    {
        get => "WMI";
    }





    /// <summary>
    ///     Execute the probe with the given parameters.
    /// </summary>
    /// <param name="parameters">
    ///     Provider-specific parameters from the rule JSON. pattern:<namespace>.<class>.<property> -
    /// </param>
    /// <param name="token"></param>
    /// <returns>ProbeResult containing the raw value and provenance.</returns>
    public async Task<ProbeResult> ExecuteAsync(IProviderParameters parameters, CancellationToken token)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        ProbeResult pr = new();
        var param = (WmiParameters)parameters;

        var wql = param.Class ?? throw new ArgumentException("Missing or invalid 'Class' parameter.");
        var ns = param.Namespace ?? "root/cimv2";
        var property = param.Property;

        _logger?.Log("INF", $"Executing WQL query: {wql} in Namespace: {ns} for property: {property}", "CimReader");

        var ps = new PowerShellRunner(_logger);

        var script = $"Get-CimInstance -Classname {wql} -Property {property} | Out-String";

        PowerShellResult? results = await ps.RunAsync(script, null, TimeSpan.FromSeconds(30), token).ConfigureAwait(false);
        if (results.Output != null)
        {
            pr.ProbeSuccess = results.Success;
            pr.Value = results.Output[0];
            pr.Provider = Provider;
        }
        else
        {
            pr.Message = results?.Exception?.Message;
        }

        return await Task.FromResult(pr);
    }







    private static bool IsInvalidQuery(CimException cex)
    {
        return cex.StatusCode == 5;
    }





    private static bool IsNamespaceUnavailable(CimException cex)
    {
        return cex.StatusCode is 3 or 1;
    }





    private static string? BuildFallbackQuery(string original)
    {
        _trace.TraceEvent(TraceEventType.Warning, 0, "Attempting to build fallback query for: {0}", original);

        if (string.IsNullOrWhiteSpace(original)) return null;

        _logger?.Log("WRN", $"Building fallback query for: {original}", "BuildFallbackQuery");


        if (Regex.IsMatch(original, "^\\s*SELECT\\s+\\*\\s+FROM\\s+", RegexOptions.IgnoreCase)) return original;

        Match match = Regex.Match(original, "^\\s*SELECT\\s+.+?\\s+FROM\\s+([\\w\\.]+)(.*)$", RegexOptions.IgnoreCase);

        if (!match.Success) return null;

        var classAndRest = match.Groups[1].Value + match.Groups[2].Value;

        return $"SELECT * FROM {classAndRest}";
    }


}
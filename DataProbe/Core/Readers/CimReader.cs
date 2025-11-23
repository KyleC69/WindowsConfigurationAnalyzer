//  Created:  2025/10/29
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




#region

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using KC.WindowsConfigurationAnalyzer.Contracts;

using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;

#endregion





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
    public string Provider => "WMI";





    /// <summary>
    ///     Execute the probe with the given parameters.
    /// </summary>
    /// <param name="parameters">Provider-specific parameters from the rule JSON.</param>
    /// <param name="callerName"></param>
    /// <param name="callerFilePath"></param>
    /// <returns>ProbeResult containing the raw value and provenance.</returns>
    public async Task<ProbeResult> ExecuteAsync(IDictionary<string, object> parameters, CancellationToken token, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        if (parameters.Count == 0)
        {
            throw new ArgumentException("No parameters provided.");
        }

        string wql = parameters.TryGetValue("Query", out object? qObj) && qObj is string qStr ? qStr : throw new ArgumentException("Missing or invalid 'Query' parameter.");
        string? ns = parameters.TryGetValue("Namespace", out object? nsObj) && nsObj is string nsStr ? nsStr : null;
        ProbeResult result = new() { Provider = Provider };
        _logger?.Log("INF", $"Executing WQL query: {wql} in Namespace: {ns}", $"QueryAsync - Caller: {callerName}, Page: {callerFilePath}");



        return await Task.FromResult<ProbeResult>(null);
    }






    public async Task<IReadOnlyList<IDictionary<string, object?>>> QueryAsync(string wql, string? scope = null, CancellationToken cancellationToken = default, [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerPage = "")
    {
        string ns = string.IsNullOrWhiteSpace(scope) ? "root/cimv2" : NormalizeNamespace(scope!);


        return await ExecuteMiAsync(ns, wql, cancellationToken, callerName).ConfigureAwait(false);
    }





    private static string NormalizeNamespace(string scope)
    {
        string trimmed = scope.Trim();
        trimmed = trimmed.StartsWith("\\\\.") ? trimmed.Substring(4) : trimmed;
        trimmed = trimmed.Replace('\\', '/');
        if (trimmed.StartsWith('/'))
        {
            trimmed = trimmed.Substring(1);
        }

        return trimmed;
    }





    // Force DCOM for namespaces known to be unavailable via WSMan (e.g., RSOP) to avoid an initial failing round trip.
    private static bool ShouldForceDcom(string ns)
    {
        return ns.StartsWith("root/rsop", StringComparison.OrdinalIgnoreCase);
    }





    private static async Task<IReadOnlyList<IDictionary<string, object?>>> ExecuteMiAsync(string ns, string wql, CancellationToken cancellationToken, [CallerMemberName] string callerName = "")
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger?.Log("INF", $"Executing MI query in namespace '{ns}': {wql}", $"ExecuteMiAsync - Caller: {callerName}");
        List<IDictionary<string, object?>> results = [];
        string current = wql;
        bool attemptedFallbackQuery = false;
        bool attemptedProtocolFallback = false;
        bool useDcom = ShouldForceDcom(ns); // initialize with forced DCOM if namespace requires it

        while (true)
        {
            using CimSession session = useDcom
                ? CimSession.Create(null, new DComSessionOptions())
                : CimSession.Create(null, new CimSessionOptions());

            try
            {
                // MI API is synchronous; wrap in Task.Run for cancellation cooperatively.
                List<CimInstance> instances = await Task.Run(
                    () => session.QueryInstances(ns, "WQL", current).ToList(),
                    cancellationToken).ConfigureAwait(false);

                foreach (CimInstance inst in instances)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Dictionary<string, object?> dict = new(StringComparer.OrdinalIgnoreCase);
                    foreach (CimProperty? p in inst.CimInstanceProperties)
                    {
                        dict[p.Name] = p.Value;
                    }

                    results.Add(dict);
                }

                break;
            }
            catch (CimException cex) when (!attemptedProtocolFallback && IsNamespaceUnavailable(cex))
            {
                _logger?.Log("ERR", $"Namespace '{ns}' unavailable via WSMan; retrying with DCOM.", "ExecuteMiAsync");
                // Retry using DCOM for namespaces not exposed via WSMan (e.g., RSOP).
                attemptedProtocolFallback = true;
                useDcom = true;
            }
            catch (CimException cex) when (!attemptedFallbackQuery && IsInvalidQuery(cex))
            {
                _logger?.Log("ERR", $"Invalid WQL query: {current}", "ExecuteMiAsync");
                string? fb = BuildFallbackQuery(current);

                if (fb == null || fb.Equals(current, StringComparison.OrdinalIgnoreCase))
                {
                    throw;
                }

                attemptedFallbackQuery = true;
                current = fb;
            }
        }

        return results.AsReadOnly();
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

        if (string.IsNullOrWhiteSpace(original))
        {
            return null;
        }

        _logger?.Log("WRN", $"Building fallback query for: {original}", "BuildFallbackQuery");


        if (Regex.IsMatch(original, "^\\s*SELECT\\s+\\*\\s+FROM\\s+", RegexOptions.IgnoreCase))
        {
            return original;
        }

        Match match = Regex.Match(original, "^\\s*SELECT\\s+.+?\\s+FROM\\s+([\\w\\.]+)(.*)$", RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return null;
        }

        string classAndRest = match.Groups[1].Value + match.Groups[2].Value;

        return $"SELECT * FROM {classAndRest}";
    }


}
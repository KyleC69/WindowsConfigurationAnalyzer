//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   AnalyzerRunner.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Reflection;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Engine;


public sealed class AnalyzerRunner
{


    private readonly List<IAnalyzerModule> _modules;





    // Constructor injection: modules are required.
    public AnalyzerRunner(IEnumerable<IAnalyzerModule> modules)
    {
        if (modules is null)
        {
            throw new ArgumentNullException(nameof(modules));
        }

        _modules = modules.Where(m => m is not null).ToList();

        if (_modules.Count == 0)
        {
            throw new ArgumentException("At least one analyzer module must be registered.", nameof(modules));
        }
    }





    public static string? ProjectDir => Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "ProjectDirectory")?.Value;





    public async Task<AnalyzerResult> RunAllAsync(string correlationId, IActivityLogger logger,
        CancellationToken cancellationToken = default)
    {
        List<AreaResult> areaResults = [];
        List<Finding> globalFindings = [];


        // Emit ETW session start per taxonomy1001
        string sessionId = Guid.NewGuid().ToString("N");
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";

        logger.Log("INF", $" Starting ETW session {sessionId}", "ETW");
        try
        {
            //  WcaDiagnosticsProvider.EventWriteSessionStart(sessionId, Environment.MachineName, version, correlationId);
        }
        catch
        {
        }

        if (_modules.Count == 0)
        {
            logger.Log("WRN", " No analyzer modules registered.", "AnalyzerRunner");

            return new AnalyzerResult(
                Environment.MachineName,
                DateTimeOffset.UtcNow,
                areaResults,
                globalFindings);
        }

        IAnalyzerContext context = null!; // = new AnalyzerContext(logger, new RegistryReader(), new CimReader(logger), new EventLogReader(), new FirewallReader(), new EnvironmentReader());

        IEnumerable<Task<AreaResult>> tasks = _modules.Select(async m =>
        {
            try
            {
                logger.Log("INF", $" Starting analyzer {m.Name}", $"{m.Area}");
                AreaResult result = await m.AnalyzeAsync(logger, context, cancellationToken).ConfigureAwait(false);
                logger.Log("INF", $" Completed analyzer {m.Name}", $"{m.Area}");

                return result;
            }
            catch (OperationCanceledException)
            {
                logger.Log("WRN", $" Analyzer {m.Name} canceled", $"{m.Area}");

                return new AreaResult(m.Area, null, null, new List<Finding>().AsReadOnly(),
                    new List<string>().AsReadOnly(), new List<string> { "Canceled" }.AsReadOnly());
            }
            catch (Exception ex)
            {
                logger.Log("ERR", $" Exception thrown running analyzer {m.Name}", $"{m.Area}");

                return new AreaResult(m.Area, null, null, new List<Finding>().AsReadOnly(),
                    new List<string>().AsReadOnly(), new List<string> { ex.Message }.AsReadOnly());
            }
        });

        AreaResult[] results = await Task.WhenAll(tasks).ConfigureAwait(false);
        areaResults.AddRange(results);


        return new AnalyzerResult(
            Environment.MachineName,
            DateTimeOffset.UtcNow,
            areaResults,
            globalFindings);
    }


}
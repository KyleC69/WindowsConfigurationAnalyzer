//  Created:  2025/11/30
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   PowerShellRunner.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

using KC.WindowsConfigurationAnalyzer.Contracts;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.PowerShell;


/// <summary>
///     Robust helper to execute PowerShell scripts with cancellation, timeout and structured results.
/// </summary>
public sealed class PowerShellRunner : IDisposable
{


    private readonly IActivityLogger? _logger;
    private bool _disposed;





    public PowerShellRunner(IActivityLogger? logger = null)
    {
        _logger = logger;
    }





    public void Dispose()
    {
        _disposed = true;
        GC.SuppressFinalize(this);
    }





    /// <summary>
    ///     Executes the provided PowerShell <paramref name="script" /> asynchronously.
    /// </summary>
    /// <param name="script">PowerShell script or command text to run.</param>
    /// <param name="parameters">Optional parameter dictionary passed to the script.</param>
    /// <param name="timeout">
    ///     Optional timeout. If null, no timeout is applied (only the provided
    ///     <paramref name="cancellationToken" /> governs cancellation).
    /// </param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>PowerShellResult describing output, errors and status.</returns>
    public async Task<PowerShellResult> RunAsync(string script,
        IDictionary<string, object>? parameters = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(script)) throw new ArgumentException("Script must not be null or empty.", nameof(script));

        ThrowIfDisposed();

        _logger?.Log("INF", $"Starting PowerShell execution. Script preview: {PreviewScript(script)}", "PowerShellRunner");



        // Create a runspace with normal/default settings and open it.
        using var runspace = RunspaceFactory.CreateRunspace();
        runspace.Open();

        using System.Management.Automation.PowerShell ps = System.Management.Automation.PowerShell.Create();

        ps.Runspace = runspace;

        // Add script and parameters in a single call to allow parameter binding.
        if (parameters is not null && parameters.Count > 0)
            ps.AddScript(script).AddParameters(parameters.ToList());
        else
            ps.AddScript(script);


        var outputs = new List<string>();
        var errors = new List<string>();
        Exception? exception = null;
        var timedOut = false;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (timeout.HasValue && timeout.Value != Timeout.InfiniteTimeSpan) linkedCts.CancelAfter(timeout.Value);

        // Register a callback so we attempt to stop the PowerShell run if the token cancels.
        using CancellationTokenRegistration reg = linkedCts.Token.Register(() =>
        {
            try
            {
                // Attempt to stop an ongoing invocation.
                ps?.Stop();
            }
            catch
            {
                // Swallow; we'll surface cancellation via exception below and log.
            }
        });

        try
        {
            // Run Invoke on thread-pool so CancellationToken can be observed; PowerShell's API is synchronous.
            Collection<PSObject>? result = await Task.Run(() =>
            {
                Collection<PSObject>? r = ps.Invoke();

                return r;
            }, linkedCts.Token).ConfigureAwait(false);

            // Collect output
            if (result is not null)
                foreach (var o in result)
                {
                    // Prefer the object's string representation; null -> empty string
                    var s = o?.BaseObject?.ToString() ?? string.Empty;
                    outputs.Add(s);
                }

            // Collect errors from the error stream (if any)
            if (ps.Streams?.Error is not null)
                foreach (ErrorRecord? er in ps.Streams.Error)
                {
                    try
                    {
                        errors.Add(er?.ToString() ?? string.Empty);
                    }
                    catch
                    {
                        // Best-effort: if formatting fails, at least capture message
                        try
                        {
                            errors.Add(er?.Exception?.Message ?? "Unknown error record");
                        }
                        catch
                        {
                            errors.Add("Unknown error record (failed formatting).");
                        }
                    }
                }

            _logger?.Log("INF", $"PowerShell execution completed. OutputLines={outputs.Count}; ErrorLines={errors.Count}", "PowerShellRunner");

            return new PowerShellResult(outputs.AsReadOnly(), errors.AsReadOnly(), null, timedOut, linkedCts.IsCancellationRequested && cancellationToken.IsCancellationRequested);
        }
        catch (OperationCanceledException oce)
        {
            timedOut = timeout.HasValue && timeout.Value != Timeout.InfiniteTimeSpan && !cancellationToken.IsCancellationRequested;
            _logger?.Log("WRN", $"PowerShell execution canceled (timedOut={timedOut}). Exception: {oce.Message}", "PowerShellRunner");

            throw;
        }
        catch (Exception ex)
        {
            exception = ex;
            _logger?.Log("ERR", $"PowerShell execution failed: {ex.Message}", "PowerShellRunner");

            // Attempt to capture any error stream material if available
            try
            {
                if (ps.Streams?.Error is not null)
                    foreach (ErrorRecord? er in ps.Streams.Error)
                    {
                        errors.Add(er?.ToString() ?? string.Empty);
                    }
            }
            catch
            {
                // Swallow; we already have exception and logged it.
            }

            return new PowerShellResult(outputs.AsReadOnly(), errors.AsReadOnly(), exception, timedOut, linkedCts.IsCancellationRequested && cancellationToken.IsCancellationRequested);
        }
    }





    private static string PreviewScript(string script)
    {
        if (string.IsNullOrEmpty(script)) return string.Empty;

        // keep preview short in logs
        var preview = script.Length > 200 ? script.Substring(0, 200) + "..." : script;

        return preview.Replace(Environment.NewLine, " ");
    }





    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PowerShellRunner));
    }


}



/// <summary>
///     Result returned by <see cref="PowerShellRunner" />.
/// </summary>
public sealed record PowerShellResult(
    IReadOnlyList<string> Output,
    IReadOnlyList<string> Errors,
    Exception? Exception,
    bool TimedOut,
    bool Cancelled)
{


    public bool Success => Exception is null && !TimedOut && !Cancelled && Errors?.Count == 0;


}
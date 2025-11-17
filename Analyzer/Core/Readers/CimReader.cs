// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  CimReader.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using KC.WindowsConfigurationAnalyzer.Contracts;

using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;




namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public sealed class CimReader : ICimReader
{
    private static readonly TraceSource _trace = new("KC.WindowsConfigurationAnalyzer.Analyzer.Core.Readers.CimReader");
    private static IActivityLogger? _logger;

    public CimReader(IActivityLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    //Why is scope always null?? TODO: investigate

    public async Task<IReadOnlyList<IDictionary<string, object?>>> QueryAsync(string wql, string? scope = null, CancellationToken cancellationToken = default, [CallerMemberName] string callerName = "", [CallerFilePath] string callerPage = "")
    {
        var ns = string.IsNullOrWhiteSpace(scope) ? "root/cimv2" : NormalizeNamespace(scope!);
        _logger.Log("INF", $"Executing WQL query: {wql} in Namespace: {ns}", $"QueryAsync - Caller: {callerName}, Page: {callerPage}");
        return await ExecuteMiAsync(ns, wql, cancellationToken, callerName).ConfigureAwait(false);
    }

    private static string NormalizeNamespace(string scope)
    {
        var trimmed = scope.Trim();
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
        _logger.Log("INF", $"Executing MI query in namespace '{ns}': {wql}", $"ExecuteMiAsync - Caller: {callerName}");
        List<IDictionary<string, object?>> results = new();
        var current = wql;
        var attemptedFallbackQuery = false;
        var attemptedProtocolFallback = false;
        var useDcom = ShouldForceDcom(ns); // initialize with forced DCOM if namespace requires it

        while (true)
        {
            using CimSession session = useDcom
                ? CimSession.Create(null, new DComSessionOptions())
                : CimSession.Create(null, new CimSessionOptions());

            try
            {
                // MI API is synchronous; wrap in Task.Run for cancellation cooperatively.
                var instances = await Task.Run(
                    () => session.QueryInstances(ns, "WQL", current).ToList(),
                    cancellationToken).ConfigureAwait(false);

                foreach (var inst in instances)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Dictionary<string, object?> dict = new(StringComparer.OrdinalIgnoreCase);
                    foreach (var p in inst.CimInstanceProperties)
                    {
                        dict[p.Name] = p.Value;
                    }
                    results.Add(dict);
                }
                break;
            }
            catch (CimException cex) when (!attemptedProtocolFallback && IsNamespaceUnavailable(cex))
            {
                _logger.Log("ERR", $"Namespace '{ns}' unavailable via WSMan; retrying with DCOM.", "ExecuteMiAsync");
                // Retry using DCOM for namespaces not exposed via WSMan (e.g., RSOP).
                attemptedProtocolFallback = true;
                useDcom = true;
                continue;
            }
            catch (CimException cex) when (!attemptedFallbackQuery && IsInvalidQuery(cex))
            {
                _logger.Log("ERR", $"Invalid WQL query: {current}", "ExecuteMiAsync");
                var fb = BuildFallbackQuery(current);
                if (fb == null || fb.Equals(current, StringComparison.OrdinalIgnoreCase))
                {
                    throw;
                }

                attemptedFallbackQuery = true;
                current = fb;
                continue; // retry
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

        _logger.Log("WRN", $"Building fallback query for: {original}", "BuildFallbackQuery");


        if (Regex.IsMatch(original, "^\\s*SELECT\\s+\\*\\s+FROM\\s+", RegexOptions.IgnoreCase))
        {
            return original;
        }

        var match = Regex.Match(original, "^\\s*SELECT\\s+.+?\\s+FROM\\s+([\\w\\.]+)(.*)$", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        var classAndRest = match.Groups[1].Value + match.Groups[2].Value;
        return $"SELECT * FROM {classAndRest}";
    }
}
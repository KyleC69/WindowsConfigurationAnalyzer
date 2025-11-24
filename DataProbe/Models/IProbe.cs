using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.WindowsConfigurationAnalyzer.DataProbe.Models;

    /// <summary>
    /// Contract for all probes (Registry, WMI, FileSystem, ACL, EventLog, etc.)
    /// </summary>
    public interface IProbe
    {
        /// <summary>
        /// Unique provider name (e.g. "Registry", "WMI", "FileSystem").
        /// Used to match against Rule.Provider in the workflow.
        /// </summary>
        string Provider { get; }





        /// <summary>
        /// Execute the probe with the given parameters.
        /// </summary>
        /// <param name="parameters">Provider-specific parameters from the rule JSON.</param>
        /// <param name="token"></param>
        /// <returns>ProbeResult containing the raw value and provenance.</returns>
        Task<ProbeResult> ExecuteAsync(IDictionary<string, object> parameters, CancellationToken token);
    }


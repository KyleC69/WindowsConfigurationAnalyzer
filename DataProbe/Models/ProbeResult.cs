using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.WindowsConfigurationAnalyzer.DataProbe.Models;

    /// <summary>
    /// Result returned by a probe before condition evaluation.
    /// </summary>
    public class ProbeResult
    {
        /// <summary>
        /// The provider that produced this result.
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// The raw value extracted (registry value, WMI property, ACL string, etc.).
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Whether the probe executed successfully (not yet condition evaluation).
        /// </summary>
        public bool ProbeSuccess { get; set; }

        /// <summary>
        /// Diagnostic or error message from the probe.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the probe executed.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional metadata (e.g. registry path, file path, WMI class).
        /// </summary>
        public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

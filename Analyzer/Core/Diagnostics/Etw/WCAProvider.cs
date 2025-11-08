// Created:  2025/11/01
// Solution: WindowsConfigurationAnalyzer
// Project:  Analyzer
// File:  WCAProvider.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Diagnostics.Tracing;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Diagnostics.Etw;



public static class WCADiagnosticsProvider
{
    //
    // Provider "WCA-Diagnostics-Provider" event count =23
    //

    private static readonly WCADiagnosticsEventSource m_provider = WCADiagnosticsEventSource.Log;
    //
    // Task : eventGUIDs
    //

    //
    // Event methods
    //





    //
    // Event method for SessionStart
    //
    public static bool EventWriteSessionStart(string SessionId, string Computer, string Version, string CorrelationId)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.SessionStart(SessionId, Computer, Version, CorrelationId);

        return true;
    }





    //
    // Event method for SessionStop
    //
    public static bool EventWriteSessionStop(string SessionId, int Areas, int Warnings, int Errors,
                                             double ElapsedSeconds, string CorrelationId)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.SessionStop(SessionId, Areas, Warnings, Errors, ElapsedSeconds, CorrelationId);

        return true;
    }





    //
    // Event method for HardwareCapture
    //
    public static bool EventWriteHardwareCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs,
                                                 string Details)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.HardwareCapture(Module, CorrelationId, ItemCount, DurationMs, Details);

        return true;
    }





    //
    // Event method for HardwareAnalysis
    //
    public static bool EventWriteHardwareAnalysis(string Module, string CorrelationId, uint FindingsCount,
                                                  string Severity, string Summary)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.HardwareAnalysis(Module, CorrelationId, FindingsCount, Severity, Summary);

        return true;
    }





    //
    // Event method for OSCapture
    //
    public static bool EventWriteOSCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs,
                                           string Details)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.OSCapture(Module, CorrelationId, ItemCount, DurationMs, Details);

        return true;
    }





    //
    // Event method for OSAnalysis
    //
    public static bool EventWriteOSAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity,
                                            string Summary)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.OSAnalysis(Module, CorrelationId, FindingsCount, Severity, Summary);

        return true;
    }





    //
    // Event method for SoftwareCapture
    //
    public static bool EventWriteSoftwareCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs,
                                                 string Details)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.SoftwareCapture(Module, CorrelationId, ItemCount, DurationMs, Details);

        return true;
    }





    //
    // Event method for SoftwareAnalysis
    //
    public static bool EventWriteSoftwareAnalysis(string Module, string CorrelationId, uint FindingsCount,
                                                  string Severity, string Summary)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.SoftwareAnalysis(Module, CorrelationId, FindingsCount, Severity, Summary);

        return true;
    }





    //
    // Event method for NetworkCapture
    //
    public static bool EventWriteNetworkCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs,
                                                string Details)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.NetworkCapture(Module, CorrelationId, ItemCount, DurationMs, Details);

        return true;
    }





    //
    // Event method for NetworkAnalysis
    //
    public static bool EventWriteNetworkAnalysis(string Module, string CorrelationId, uint FindingsCount,
                                                 string Severity, string Summary)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.NetworkAnalysis(Module, CorrelationId, FindingsCount, Severity, Summary);

        return true;
    }





    //
    // Event method for SecurityCapture
    //
    public static bool EventWriteSecurityCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs,
                                                 string Details)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.SecurityCapture(Module, CorrelationId, ItemCount, DurationMs, Details);

        return true;
    }





    //
    // Event method for SecurityAnalysis
    //
    public static bool EventWriteSecurityAnalysis(string Module, string CorrelationId, uint FindingsCount,
                                                  string Severity, string Summary)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.SecurityAnalysis(Module, CorrelationId, FindingsCount, Severity, Summary);

        return true;
    }





    //
    // Event method for PerformanceCapture
    //
    public static bool EventWritePerformanceCapture(string Module, string CorrelationId, uint ItemCount,
                                                    uint DurationMs, string Details)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.PerformanceCapture(Module, CorrelationId, ItemCount, DurationMs, Details);

        return true;
    }





    //
    // Event method for PerformanceAnalysis
    //
    public static bool EventWritePerformanceAnalysis(string Module, string CorrelationId, uint FindingsCount,
                                                     string Severity, string Summary)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.PerformanceAnalysis(Module, CorrelationId, FindingsCount, Severity, Summary);

        return true;
    }





    //
    // Event method for StartupCapture
    //
    public static bool EventWriteStartupCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs,
                                                string Details)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.StartupCapture(Module, CorrelationId, ItemCount, DurationMs, Details);

        return true;
    }





    //
    // Event method for StartupAnalysis
    //
    public static bool EventWriteStartupAnalysis(string Module, string CorrelationId, uint FindingsCount,
                                                 string Severity, string Summary)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.StartupAnalysis(Module, CorrelationId, FindingsCount, Severity, Summary);

        return true;
    }





    //
    // Event method for AuditCapture
    //
    public static bool EventWriteAuditCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs,
                                              string Details)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.AuditCapture(Module, CorrelationId, ItemCount, DurationMs, Details);

        return true;
    }





    //
    // Event method for AuditAnalysis
    //
    public static bool EventWriteAuditAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity,
                                               string Summary)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.AuditAnalysis(Module, CorrelationId, FindingsCount, Severity, Summary);

        return true;
    }





    //
    // Event method for ExportCapture
    //
    public static bool EventWriteExportCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs,
                                               string Details)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.ExportCapture(Module, CorrelationId, ItemCount, DurationMs, Details);

        return true;
    }





    //
    // Event method for ExportAnalysis
    //
    public static bool EventWriteExportAnalysis(string Module, string CorrelationId, uint FindingsCount,
                                                string Severity, string Summary)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.ExportAnalysis(Module, CorrelationId, FindingsCount, Severity, Summary);

        return true;
    }





    //
    // Event method for ExportCompleted
    //
    public static bool EventWriteExportCompleted(string SessionId, string Format, string Path, string CorrelationId)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.ExportCompleted(SessionId, Format, Path, CorrelationId);

        return true;
    }





    //
    // Event method for ReadersCapture
    //
    public static bool EventWriteReadersCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs,
                                                string Details)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.ReadersCapture(Module, CorrelationId, ItemCount, DurationMs, Details);

        return true;
    }





    //
    // Event method for ReadersAnalysis
    //
    public static bool EventWriteReadersAnalysis(string Module, string CorrelationId, uint FindingsCount,
                                                 string Severity, string Summary)
    {
        if (!m_provider.IsEnabled())
        {
            return true;
        }

        m_provider.ReadersAnalysis(Module, CorrelationId, FindingsCount, Severity, Summary);

        return true;
    }
}




internal sealed class WCADiagnosticsEventSource : EventSource
{
    public static readonly WCADiagnosticsEventSource Log = new();





    private WCADiagnosticsEventSource() : base("WCA-Diagnostics-Provider")
    {
    }





    [Event(0x3e9, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void SessionStart(string SessionId, string Computer, string Version, string CorrelationId)
    {
        if (IsEnabled())
        {
            WriteEvent(0x3e9, SessionId, Computer, Version, CorrelationId);
        }
    }





    [Event(0x3ea, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void SessionStop(string SessionId, int Areas, int Warnings, int Errors, double ElapsedSeconds,
                            string CorrelationId)
    {
        if (IsEnabled())
        {
            WriteEvent(0x3ea, new object[] { SessionId, Areas, Warnings, Errors, ElapsedSeconds, CorrelationId });
        }
    }





    [Event(0x7d1, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void HardwareCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        if (IsEnabled())
        {
            WriteEvent(0x7d1, new object[] { Module, CorrelationId, ItemCount, DurationMs, Details });
        }
    }





    [Event(0x7d2, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void HardwareAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity,
                                 string Summary)
    {
        if (IsEnabled())
        {
            WriteEvent(0x7d2, new object[] { Module, CorrelationId, FindingsCount, Severity, Summary });
        }
    }





    [Event(0x899, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void OSCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        if (IsEnabled())
        {
            WriteEvent(0x899, new object[] { Module, CorrelationId, ItemCount, DurationMs, Details });
        }
    }





    [Event(0x89a, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void OSAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity, string Summary)
    {
        if (IsEnabled())
        {
            WriteEvent(0x89a, new object[] { Module, CorrelationId, FindingsCount, Severity, Summary });
        }
    }





    [Event(0x961, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void SoftwareCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        if (IsEnabled())
        {
            WriteEvent(0x961, new object[] { Module, CorrelationId, ItemCount, DurationMs, Details });
        }
    }





    [Event(0x962, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void SoftwareAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity,
                                 string Summary)
    {
        if (IsEnabled())
        {
            WriteEvent(0x962, new object[] { Module, CorrelationId, FindingsCount, Severity, Summary });
        }
    }





    [Event(0xA29, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void NetworkCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        if (IsEnabled())
        {
            WriteEvent(0xA29, new object[] { Module, CorrelationId, ItemCount, DurationMs, Details });
        }
    }





    [Event(0xA2A, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void NetworkAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity,
                                string Summary)
    {
        if (IsEnabled())
        {
            WriteEvent(0xA2A, new object[] { Module, CorrelationId, FindingsCount, Severity, Summary });
        }
    }





    [Event(0xAF1, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void SecurityCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        if (IsEnabled())
        {
            WriteEvent(0xAF1, new object[] { Module, CorrelationId, ItemCount, DurationMs, Details });
        }
    }





    [Event(0xAF2, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void SecurityAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity,
                                 string Summary)
    {
        if (IsEnabled())
        {
            WriteEvent(0xAF2, new object[] { Module, CorrelationId, FindingsCount, Severity, Summary });
        }
    }





    [Event(0xBB9, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void PerformanceCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        if (IsEnabled())
        {
            WriteEvent(0xBB9, new object[] { Module, CorrelationId, ItemCount, DurationMs, Details });
        }
    }





    [Event(0xBBA, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void PerformanceAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity,
                                    string Summary)
    {
        if (IsEnabled())
        {
            WriteEvent(0xBBA, new object[] { Module, CorrelationId, FindingsCount, Severity, Summary });
        }
    }





    [Event(0xC81, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void StartupCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        if (IsEnabled())
        {
            WriteEvent(0xC81, new object[] { Module, CorrelationId, ItemCount, DurationMs, Details });
        }
    }





    [Event(0xC82, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void StartupAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity,
                                string Summary)
    {
        if (IsEnabled())
        {
            WriteEvent(0xC82, new object[] { Module, CorrelationId, FindingsCount, Severity, Summary });
        }
    }





    [Event(0xD49, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void AuditCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        if (IsEnabled())
        {
            WriteEvent(0xD49, new object[] { Module, CorrelationId, ItemCount, DurationMs, Details });
        }
    }





    [Event(0xD4A, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void AuditAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity, string Summary)
    {
        if (IsEnabled())
        {
            WriteEvent(0xD4A, new object[] { Module, CorrelationId, FindingsCount, Severity, Summary });
        }
    }





    [Event(0xE11, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void ExportCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        if (IsEnabled())
        {
            WriteEvent(0xE11, new object[] { Module, CorrelationId, ItemCount, DurationMs, Details });
        }
    }





    [Event(0xE12, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void ExportAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity, string Summary)
    {
        if (IsEnabled())
        {
            WriteEvent(0xE12, new object[] { Module, CorrelationId, FindingsCount, Severity, Summary });
        }
    }





    [Event(0xE1A, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void ExportCompleted(string SessionId, string Format, string Path, string CorrelationId)
    {
        if (IsEnabled())
        {
            WriteEvent(0xE1A, SessionId, Format, Path, CorrelationId);
        }
    }





    [Event(0xED9, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void ReadersCapture(string Module, string CorrelationId, uint ItemCount, uint DurationMs, string Details)
    {
        if (IsEnabled())
        {
            WriteEvent(0xED9, new object[] { Module, CorrelationId, ItemCount, DurationMs, Details });
        }
    }





    [Event(0xEDA, Level = EventLevel.Informational, Keywords = Keywords.None)]
    public void ReadersAnalysis(string Module, string CorrelationId, uint FindingsCount, string Severity,
                                string Summary)
    {
        if (IsEnabled())
        {
            WriteEvent(0xEDA, new object[] { Module, CorrelationId, FindingsCount, Severity, Summary });
        }
    }





    // Keywords can be expanded if filtering is required in the future
    public class Keywords
    {
        public const EventKeywords None = 0;
    }
}
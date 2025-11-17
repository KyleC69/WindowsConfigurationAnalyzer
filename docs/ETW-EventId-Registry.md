# WCA ETW Event ID Registry

This document lists Event IDs implemented in the codebase and their purpose, aligned to the Event Taxonomy in AnalyzerArchitecture.md.

Guidelines
- Event IDs are stable4-digit numeric values (no computed composites).
- Reserve200 IDs per major module/area to allow future events without renumbering.
- Blocks are inclusive ranges; follow the allocation map below.
- Update this registry whenever the manifest is changed.

Allocation blocks (200 IDs each)
-1000-1199 Security
-1200-1399 System/OS
-1400-1599 Hardware
-2000-2199 UI
-2200-2399 Host/Runtime
-2300-2499 Detectors (subset of Host/Runtime for historical compatibility)
-3100-3299 Validation
-4000-4199 Deployment
-5000-5199 Versioning
-6000-6199 Storage/Database
-7000-7199 Self-Heal/Remediation
-8000-8199 Audit/User Actions
-8100-8299 AI/Inference (sub-block)
-8200-8399 Performance/Health
-8400-8599 Policy/Configuration

Source-of-truth
- The manifests under `Analyzer/Core/Diagnostics/Etw/Manifest/` are the authoritative source of event definitions and localized strings.
- Keep this registry in sync with any changes to those manifests.

Existing events (kept)

Security (1000-1199)
-1000 SecurityAccessEvaluated: Informational - Security Evaluation result (principal, action, resource, decision, reason)
-1001 SecurityAccessDenied: Warning - Denied access (principal, action, resource, reason)

UI (2000-2199)
-2000 UI_ButtonClicked: Informational - UI interaction telemetry (control, page, user)
-2001 UI_Error: Error - UI error with exception (control, page, message, exception)

Host/Runtime (2200-2399)
-2200 HOST_OPS: Informational - Generic host message
-2201 HostStarted: Informational - Host started (version, uptime, correlation)
-2202 HostStopping: Informational - Host stopping
-2210 HOST_ERR: Error - Host error (message)
-2250 GeneralException: Error - Unhandled exception (message, exception, stack)

Detectors (2300-2499)
-2300 DetectorLoaded: Informational - Detector loaded metadata
-2301 DetectorInitialized: Informational - Detector initialization success
-2302 DetectorInitializationFailed: Error - Detector init failed
-2303 DetectorHeartbeat: Informational - Detector health heartbeat
-2310 DetectorDeployed: Informational - Detector deployment
-2311 DetectorDeploymentFailed: Error - Deployment failure

Validation (3100-3299)
-3100 ValidationRunSummary: Informational - Validation run summary (ruleset, passed, failed, warnings, duration)

Deployment (4000-4199)
-4000 DeploymentStarted: Informational - Deployment started
-4001 DeploymentCompleted: Informational - Deployment completed
-4002 DeploymentFailed: Error - Deployment failed

Versioning (5000-5199)
-5000 VersionSnapshotCreated: Informational - Snapshot created

Storage/Database (6000-6199)
-6000 StorageOperationCompleted: Informational - Storage operation success
-6001 StorageOperationFailed: Error - Storage operation failure
-6020 DBQueryExecuted: Informational - Database query executed
-6021 DBWriteFailed: Error - Database write failure

Self-Heal (7000-7199)
-7000 SelfHealActionAttempted: Informational - Attempted remediation
-7001 SelfHealActionResult: Informational - Remediation result

Audit/User Actions (8000-8199)
-8000 AuditActionRecorded: Informational - Auditable action recorded
-8010 AuditCommandSuccess: Informational - Command executed successfully
-8020 AuditCommandFailure: Warning - Command execution failed

AI/Perf/Health/Policy
-8100 AIInferenceCompleted: Informational - AI inference completed
-8200 PerfMetricCollected: Informational - Performance metric
-8300 HostHealthSample: Informational - Host health sample
-8400 PolicyChanged: Informational - Policy changed

---

WCA manifest additions (added per Analyzer/Core/Diagnostics/Etw/Manifest/WCA.Diagnostics.Provider.man)

Core lifecycle
-1001 SessionStart: Informational - Session started (sessionId, computer, version, correlationId)
-1002 SessionStop: Informational - Session stopped (sessionId, areas, warnings, errors, elapsedSeconds, correlationId)

Per-module Capture and Analysis events (one Capture + one Analysis per module)

Hardware block (2000-2199)
-2001 HardwareCapture: Informational - Hardware capture completed (Module, CorrelationId, ItemCount, DurationMs, Details)
-2002 HardwareAnalysis: Informational - Hardware analysis completed (Module, CorrelationId, FindingsCount, Severity, Summary)

OS block (2200-2399)
-2201 OSCapture: Informational - OS capture completed (Module, CorrelationId, ItemCount, DurationMs, Details)
-2202 OSAnalysis: Informational - OS analysis completed (Module, CorrelationId, FindingsCount, Severity, Summary)

Software block (2400-2599)
-2401 SoftwareCapture: Informational - Software inventory capture completed (Module, CorrelationId, ItemCount, DurationMs, Details)
-2402 SoftwareAnalysis: Informational - Software analysis completed (Module, CorrelationId, FindingsCount, Severity, Summary)

Network block (2600-2799)
-2601 NetworkCapture: Informational - Network capture completed (Module, CorrelationId, ItemCount, DurationMs, Details)
-2602 NetworkAnalysis: Informational - Network analysis completed (Module, CorrelationId, FindingsCount, Severity, Summary)

Security block (2800-2999)
-2801 SecurityCapture: Informational - Security snapshot captured (Module, CorrelationId, ItemCount, DurationMs, Details)
-2802 SecurityAnalysis: Informational - Security analysis completed (Module, CorrelationId, FindingsCount, Severity, Summary)

Performance block (3000-3199)
-3001 PerformanceCapture: Informational - Performance counters captured (Module, CorrelationId, ItemCount, DurationMs, Details)
-3002 PerformanceAnalysis: Informational - Performance analysis completed (Module, CorrelationId, FindingsCount, Severity, Summary)

Startup/Autoruns block (3200-3399)
-3201 StartupCapture: Informational - Startup/autoruns capture completed (Module, CorrelationId, ItemCount, DurationMs, Details)
-3202 StartupAnalysis: Informational - Startup analysis completed (Module, CorrelationId, FindingsCount, Severity, Summary)

Audit/EventLog block (3400-3599)
-3401 AuditCapture: Informational - Event log summary captured (Module, CorrelationId, ItemCount, DurationMs, Details)
-3402 AuditAnalysis: Informational - Event log analysis completed (Module, CorrelationId, FindingsCount, Severity, Summary)

Exporters block (3600-3799)
-3601 ExportCapture: Informational - Export module prepared artifacts (Module, CorrelationId, ItemCount, DurationMs, Details)
-3602 ExportAnalysis: Informational - Export module validation completed (Module, CorrelationId, FindingsCount, Severity, Summary)
-3610 ExportCompleted: Informational - Export completed (SessionId, Format, Path, CorrelationId)

Readers/IO block (3800-3999)
-3801 ReadersCapture: Informational - Readers/IO capture completed (Module, CorrelationId, ItemCount, DurationMs, Details)
-3802 ReadersAnalysis: Informational - Readers/IO analysis completed (Module, CorrelationId, FindingsCount, Severity, Summary)

Notes
- The manifests under `Analyzer/Core/Diagnostics/Etw/Manifest/` are authoritative; keep this registry synchronized with them.
- All forward-facing strings (task/opcode/event messages) must be present in the manifest stringTable for localization.
- Event IDs are stable and must not be computed at runtime.

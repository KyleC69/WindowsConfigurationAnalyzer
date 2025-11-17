# Windows Configuration Analyzer (WCA) – Implementation Plan

Version:1.0
Status: Draft
Scope: Implement the read-only, modular C# library described in AnalyzerArchitecture.md targeting .NET9

Repo Conventions and Build (Directory.Build.props and RootNamespace)
- Do not modify `Directory.Build.props`. Project-wide settings come from that file.
- RootNamespace pattern: UI/host projects may use their own `RootNamespace` (e.g., `Analyzer`). Library code uses explicit namespaces starting with `WindowsConfigurationAnalyzer` to avoid coupling to project `RootNamespace`.
- New project naming: name by function (e.g., `Core`, `Engine`, `Export`, `Diagnostics`, `Readers`, `Reports`) and do not include the full product name “Windows Configuration Analyzer”.
- Code should use explicit namespace declarations and not rely on default `RootNamespace` generation.

1) Objectives and Non-Goals
- Objectives
 - Deliver a modular, testable, read-only analyzer library that enumerates Windows configuration, detects anomalies, and exports JSON and HTML reports.
 - Provide structured logging and an action log for traceability.
 - Ensure extensibility via DI and plugin-like analyzer modules.
- Non-Goals
 - No configuration changes to the system (strictly read-only).
 - No heavy kernel tracing or privileged-only features by default.

2) Architectural Baseline
- .NET9 class library `Analyzer` (existing), nullable enabled, analyzers enforced.
- Namespaces (as per architecture document):
 - Core: `WindowsConfigurationAnalyzer` (engine, contracts, models)
 - Areas: `WindowsConfigurationAnalyzer.Hardware|Software|Network|Security|Performance|OS`
 - Cross-cutting: `...Export`, `...Logging`, `...Diagnostics`, `...Utilities`
- Key patterns: Strategy (detectors), Factory/Provider (module creation), Observer (logging/events), DI, async/parallelism.

3) Deliverables
- Core library with public contracts and XML docs
- Area analyzers (MVP set + expanded set)
- JSON exporter with atomic writes
- HTML report builder using Razor templates
- Anomaly detection engine (rule-based + hooks for statistical/ML later)
- Structured logging with in-memory action log and pluggable providers
- Unit and integration tests; sample runner app (optional but recommended)

4) Phased Plan and Work Breakdown

Phase0 – Repo/Build/Quality (Day0–1)
- Add `.editorconfig`, enable nullable, treat warnings as errors for project.
- Add code analyzers (Microsoft.CodeAnalysis.NetAnalyzers) and StyleCop (optional ruleset).
- Set up Directory.Build.props for consistent LangVersion, Nullable, Deterministic. (Note: follow existing file, do not alter.)
- Add CI workflow (build, test) if applicable.
- Acceptance: Clean build on .NET9; analyzers active; tests project(s) scaffolded.

Phase1 – Core Contracts and Models (Day1–3)
- Contracts
 - `IAnalyzerModule` (Name, Area, `Task<AreaResult> AnalyzeAsync(IAnalyzerContext, CancellationToken)`)
 - `IAnalyzerContext` (ILogger, time provider, readers: file system, registry, WMI/CIM, event log, firewall, environment)
 - `IAnomalyDetector` and `IRule` minimal contracts
 - `IExporter` (Export/WriteAsync)
- Models
 - `AnalyzerResult` (system info, per-area results, global anomalies, action log)
 - `AreaResult` (summary, details, anomalies, warnings, errors)
 - `ActionLogEntry` (timestamp, area, action, level, message, exception)
 - `Finding`/`AnomalyFinding` (severity enum: Info/Warning/Critical, message, context)
- Acceptance: Compile-time-only; unit tests for model serialization using System.Text.Json.

Phase2 – Infrastructure and Readers (Day3–7)
- Logging
 - Integrate `Microsoft.Extensions.Logging` abstractions; support scopes.
 - Implement `ActionLogger` that mirrors entries to in-memory list + `ILogger`.
- Readers (wrappers for testability)
 - `IFileSystem` via System.IO.Abstractions
 - `IRegistryReader` (readonly keys, value fetch helpers)
 - `ICimReader` (CIM/WMI select queries, materialization helpers)
 - `IEventLogReader` (read-only security/system/application summaries)
 - `IFirewallReader` (enumerate profiles and rules via COM wrapper)
 - `IEnvReader` (machine/user env, OS info)
- DI Setup
 - Extension `AddWcaCore(this IServiceCollection)` providing default readers/providers.
- Acceptance: Basic smoke tests with fakes/mocks for each reader; graceful exception handling pattern.

Phase3 – Exporters (Day7–10)
- JSON Exporter
 - `JsonExporter` using System.Text.Json source generation; atomic writes (temp then move).
 - Filename scheme and directory layout; UTC ISO-8601 timestamps.
- HTML Report Builder
 - Use RazorLight (RazorEngine alternative compatible with modern .NET).
 - Base layout + partials for each area; simple CSS; embed action log.
 - Minimal dashboard (counts of critical/warnings/passed checks).
- Acceptance: Golden/snapshot tests for exported JSON; Verify-based snapshot tests for HTML.

Phase4 – Analyzer Engine (Day10–12)
- `AnalyzerEngine`
 - Register modules; run sequential or parallel (`Task.WhenAll`) with cancellation.
 - Centralized exception handling with per-area error capture; action scoping.
 - Clock/time provider abstraction for test determinism.
- Acceptance: Engine runs with stub modules; action log scoped per area; concurrency tested.

Phase5 – MVP Area Analyzers (Day12–18)
- `OSAnalyzer` (OS version, uptime, services summary)
- `HardwareAnalyzer` (CPU, memory, disk inventory via Win32 classes)
- `NetworkAnalyzer` (adapters, IP config, DNS, basic firewall profile state)
- Each analyzer:
 - Read-only, resilient to access issues; produce summaries and details.
 - Emit action log entries per major step.
- Acceptance: Live run on a test machine as standard user; JSON/HTML populated for these areas.

Phase6 – Security and Software Analyzers (Day18–23)
- `SecurityAnalyzer` (AV status via Security Center, policies, audit log settings, BitLocker status if available)
- `SoftwareAnalyzer` (installed products, updates, running processes)
- Acceptance: Warnings/anomalies surfaced for common misconfigs; graceful fallbacks if access denied.

Phase7 – Performance Analyzer (Day23–26)
- `PerformanceAnalyzer` (Perf counters snapshot; boot time; service startup lag hints where possible)
- Acceptance: Counters captured with timeouts; no long-running subscriptions.

Phase8 – Anomaly Detection (Rule-Based) (Day26–30)
- `RuleEngine` with pluggable `IRule` per area; severity mapping; config overrides.
- Starter rules (examples)
 - Security: AV missing/outdated; firewall disabled; weak password policy
 - Network: Duplicate DNS; public profile with broad inbound RDP; multiple gateways conflict
 - OS/Perf: Low free disk on system drive; high CPU snapshot; memory pressure
- Acceptance: Unit tests for rules; sample baselines; rules toggled via config.

Phase9 – Testing, Tooling, and Samples (Day30–34)
- Tests
 - xUnit projects: `Analyzer.Tests`, `Analyzer.IntegrationTests`
 - Mocks: System.IO.Abstractions.TestingHelpers, custom fake readers, Moq
 - HTML snapshot tests (Verify.Xunit)
- Sample Runner (optional but recommended)
 - `samples/Wca.SampleRunner` minimal console using DI to run all modules and write exports.
- Acceptance:80%+ line/branch coverage on core; successful integration run producing both exports.

Phase10 – Hardening and Packaging (Day34–38)
- Robust exception mapping to findings; log filtering; configuration via options classes.
- Public XML docs; README; CHANGELOG; versioning (SemVer).
- NuGet packaging metadata; symbols/source link; strong naming optional.
- Acceptance: NuGet package pack succeeds; documentation finalized.

5) Public Contracts (Abbreviated)
- `IAnalyzerModule` with `string Name`, `string Area`, `Task<AreaResult> AnalyzeAsync(IAnalyzerContext ctx, CancellationToken ct)`
- `IAnalyzerContext` exposing: `ILogger`, `IFileSystem`, `IRegistryReader`, `ICimReader`, `IEventLogReader`, `IFirewallReader`, `IEnvReader`, `ITimeProvider`, `ActionLogger`
- `AnalyzerEngine` with `AddModule(IAnalyzerModule)` and `Task<AnalyzerResult> RunAllAsync(...)`
- `IExporter` with `Task ExportAsync(AnalyzerResult result, string targetPath, CancellationToken ct)`
- `IAnomalyDetector` with `IReadOnlyList<Finding> Detect(AnalyzerResult or Area models)`

6) NuGet Dependencies
- Microsoft.Extensions.Logging and Abstractions
- Microsoft.Extensions.DependencyInjection
- System.Management (WMI)
- Microsoft.Management.Infrastructure (CIM)
- System.IO.Abstractions (+ TestingHelpers)
- System.Text.Json (inbox; with source generators)
- RazorLight (HTML templates)
- System.Diagnostics.EventLog
- WindowsFirewallHelper (or COM interop for `HNetCfg.FwPolicy2`)
- Moq (tests), Verify.Xunit (HTML snapshots)

7) Concurrency, Resilience, and Safety
- All analyzers are read-only; avoid P/Invoke write paths.
- Timeouts and cancellation for WMI/CIM queries.
- Retry with backoff on transient WMI provider failures (bounded attempts).
- Guard all readers with access-denied handling; convert to warnings in results.

8) File/Export Strategy
- Atomic writes: write to temp file then `File.Move` (replace).
- Paths: `/exports/<Computer>/<YYYY-MM-DD>/WCA-<Computer>-<UTCStamp>.json|html`
- All timestamps: UTC ISO8601.

9) Testing Strategy
- Unit tests for each reader via fakes/mocks.
- Rule tests with input fixtures.
- Integration: run with mocked providers and a small live scenario (opt-in) guarded by traits.
- HTML snapshot tests to prevent regression in report layout.

10) Risks and Mitigations
- WMI/CIM variability: implement feature detection and class existence checks.
- Permissions: degrade gracefully; publish required capability notes in results.
- Firewall COM interop instability: wrap with adapter and timeouts; allow disabling.
- HTML template brittleness: use snapshot tests and small partials per area.

11) Definition of Done
- Library builds clean with analyzers; unit/integration tests pass.
- JSON and HTML exporters produce validated artifacts for MVP analyzers.
- Baseline rule set enabled and configurable; action log is captured end-to-end.
- Public contracts documented; sample runner demonstrates library usage.

12) High-Level Timeline (Approx.)
- Weeks1–2: Phases0–4
- Week3: Phases5–6
- Week4: Phases7–8
- Week5: Phases9–10, polish and docs

13) Suggested Initial Tasks (Backlog Seeds)
- Core
 - Create `IAnalyzerModule`, `AnalyzerEngine`, `AnalyzerResult`, `AreaResult`, `ActionLogEntry`.
 - Implement `ActionLogger` and DI registration extension.
- Readers
 - Implement `RegistryReader`, `CimReader`, `EventLogReader`, `FirewallReader` (adapter), `EnvReader`.
- Export
 - Implement `JsonExporter` with atomic writes.
 - Scaffold RazorLight templates and `HtmlReportBuilder`.
- Analyzers (MVP)
 - `OSAnalyzer`, `HardwareAnalyzer`, `NetworkAnalyzer` with basic data points.
- Rules
 - Implement baseline rule engine with a handful of rules per MVP analyzer.
- Tests
 - Add xUnit projects; add mocks; add initial snapshot tests.

14) Example Usage (Sample Runner)
- Build DI container, add `AddWcaCore`, register analyzers, run `AnalyzerEngine.RunAllAsync`, call exporters.

15) Documentation
- Keep `Analyzer/Docs/` updated: architecture, this plan, public contracts overview, templates guide, testing guide.

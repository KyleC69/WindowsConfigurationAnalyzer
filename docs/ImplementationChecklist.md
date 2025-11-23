# Windows Configuration Analyzer - Implementation Checklist and Project Status

Status date:2025-10-30
Target framework: .NET9
Scope: Analyzer library only (no UI code in Analyzer)

Implementation checklist (from architecture docs)
- Core/Engine
 - [x] Parallel module orchestration with scoped logging and cancellation handling
 - [x] Time abstraction via `ITimeProvider`
 - [x] Aggregate `AreaResult` into `AnalyzerResult` with action log
 - [ ] Add explicit result schema version field
- Dependency Injection
 - [x] `AddWcaCore` registers infra/readers/context
 - [ ] Configuration options to enable/disable modules and to set export paths
 - [ ] Hosted usage guidance and proper scoping when resolving `IAnalyzerContext`
- Readers (read-only abstractions)
 - [x] `EnvironmentReader` (`IEnvReader`)
 - [x] `RegistryReader` (`IRegistryReader`) – opens keys read-only
 - [x] `CimReader` (`ICimReader`) – WMI/CIM queries
 - [x] `EventLogReader` (`IEventLogReader`) – basic summaries
 - [x] `FirewallReader` (`IFirewallReader`) – FwPolicy2 COM
 - [ ] File system reader abstraction (if future analyzers require it)
- Analyzer Modules
 - [x] `OSAnalyzer`
 - [x] `HardwareAnalyzer`
 - [x] `NetworkAnalyzer`
 - [x] `SecurityAnalyzer`
 - [x] `SoftwareAnalyzer`
 - [ ] `PerformanceAnalyzer` (counters/ETW)
 - [ ] `EventLog/AuditAnalyzer` (beyond summaries)
 - [ ] `Policy/GPOAnalyzer`
 - [ ] `Startup/AutorunsAnalyzer`
- Diagnostics and Rules
 - [x] `RuleEngine` evaluates `IRule`
 - [x] Starter rules: `SEC-AV-MISSING`, `NET-DNS-DUP`
 - [ ] Expand rule set to cover CIS/NIST-aligned checks
 - [ ] Optional statistical/ML anomaly scoring
- Export
 - [x] `JsonExporter` – indented, ignore nulls, atomic write
 - [x] `HtmlReportBuilder` – summary, per-area sections, action log
 - [ ] Template-driven HTML (Razor) for maintainable theming/partials
 - [ ] JSON schema/versioning docs and example artifacts
- Logging
 - [x] `ActionLogger` with levels, sinks to `ILogger`
 - [x] Engine run uses logging scopes
 - [ ] Configurable log level filters per area/module
- Reliability and Safety
 - [x] Read-only access enforced across readers
 - [x] Module exceptions captured; run continues
 - [ ] Timeouts and cancellation propagation for long operations
- Testing
 - [x] Engine happy-path test present
 - [ ] Analyzer unit tests with mocked readers
 - [ ] Exporter tests (JSON schema validation, HTML snapshot/diff)
 - [ ] Permission scenario tests (standard user vs elevated)
- Documentation
 - [x] Architecture blueprint (`Analyzer/Docs/AnalyzerArchitecture.md`)
 - [x] Feature catalog (`Analyzer/Docs/Features.md`)
 - [ ] Keep maintenance log current as features evolve
 - [ ] Extension guides with code samples for new analyzers/readers/rules

Current project status (summary)
- Implemented
 - Engine: `AnalyzerEngine` orchestrates modules in parallel; aggregates results; uses scopes.
 - Contracts/Models: `IAnalyzerModule`, `IAnalyzerContext`, `IExporter`, `IRule`; `AnalyzerResult`, `AreaResult`, `ActionLogEntry`, `Finding`.
 - Infrastructure: `ActionLogger`, `SystemTimeProvider`.
 - Readers: `EnvironmentReader`, `RegistryReader`, `CimReader`, `EventLogReader`, `FirewallReader`.
 - Analyzers: OS, Hardware, Network, Security, Software.
 - Diagnostics: `RuleEngine` + `AvMissingRule`, `DuplicateDnsRule`.
 - Export: `JsonExporter` (atomic), `HtmlReportBuilder` (simple summary + details-as-JSON + action log).
 - DI: `AddWcaCore` registers infra/readers/context; `Analyzer/Startup.cs` registers analyzers.
 - Demo: `DemoRun.RunOnceAsync` executes, evaluates rules, exports to `exports/{Machine}/{yyyy-MM-dd}/`.

- Gaps vs blueprint
 - Missing analyzers for performance, policy/GPO, startup/autoruns, and a richer event log/audit pass.
 - Limited ruleset; needs broader per-area checks aligned to best practices.
 - HTML export not template-driven; consider Razor templates and theming.
 - JSON schema lacks explicit versioning; add version field and schema tests.
 - Minimal tests; add analyzer-level tests and exporter snapshot/schema tests.
 - No options/config to toggle modules or configure export paths.
 - Reader calls lack explicit timeouts; ensure cancellation and sensible default time limits.

- Quality and safety
 - Read-only readers and defensive exception handling are in place.
 - Atomic writes for JSON/HTML exports.
 - DI context lifetime is Scoped; demo uses root provider (acceptable for dev), consider scopes in hosted apps.

- Code health notes
 - `Analyzer/Class1.cs` appears unused and can be removed when cleanup is scheduled.
 - Add XML docs gradually to public types in core namespaces.

Next actions (priority order)
1) Add `PerformanceAnalyzer`; begin with basic counters and optional ETW hooks (read-only capture metadata only).
2) Add `Policy/GPOAnalyzer`, `Startup/AutorunsAnalyzer`, and expand Event Log analysis.
3) Introduce `WcaOptions` with module toggles and export path configuration; bind via DI.
4) Add `SchemaVersion` to `AnalyzerResult`; document and test JSON schema.
5) Move HTML export to Razor templates and introduce partials per area.
6) Expand rule set; organize by area and severity; add tests per rule.
7) Testing: add analyzer unit tests (mocked readers), exporter snapshot tests, and permission-path tests.
8) Implement timeouts on reader operations and ensure cancellation propagation.
9) Cleanup dead code and add XML docs accordingly.

Maintenance note
- Update this checklist and status after each feature change. Keep the Feature Catalog (Features.md) and this file in sync.

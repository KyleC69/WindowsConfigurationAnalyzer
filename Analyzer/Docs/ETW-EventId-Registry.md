# WCA ETW Event ID Registry

This document lists Event IDs implemented in the codebase and their purpose, aligned to the Event Taxonomy in AnalyzerArchitecture.md.

Schema: EventID = (AreaCode ×1000) + (TaskCode ×100) + Sequence

Area codes
-1 Core/Engine
-11 Exporters
-13 Diagnostics/Rules/ETW

Events
-1001 Core/Engine Control Start – SessionStart(sessionId, computer, version)
-1002 Core/Engine Control Stop – SessionStop(sessionId, areas, warnings, errors, elapsedSeconds)
-11501 Exporters Export Info – ExportCompleted(sessionId, format, path)
-13301 Diagnostics Warning Info – Warning(area, action, message)
-13401 Diagnostics Error Info – Error(area, action, message, exception)

Generic helpers (computed IDs)
- Discovery(areaCode, seq, area, action, message) – emits (areaCode×1000)+(1×100)+seq
- Analysis(areaCode, seq, area, action, message) – emits (areaCode×1000)+(2×100)+seq
- ExportInfo(areaCode, seq, area, format, path) – emits (areaCode×1000)+(5×100)+seq
- Integration(areaCode, seq, tool, args, exitCode) – emits (areaCode×1000)+(6×100)+seq

Channels and Keywords
- Channel Operational for all declared events
- Keywords: Engine, Export, Diagnostics as appropriate

Note
- Keep this file and the .man manifest in sync with code when adding new events.

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Diagnostics.Etw;

public static class EtwAreaCodes
{
    // Matches Event Taxonomy area blocks in AnalyzerArchitecture.md
    //1 Core/Engine,2 OS,3 Hardware,4 Network,5 Security,6 Software,
    //7 Performance,8 Policy/GPO,9 Startup/Autoruns,10 EventLog/Audit,
    //11 Exporters,12 Readers/IO,13 Diagnostics/Rules/ETW
    public static int Resolve(string area) => area?.Trim().ToLowerInvariant() switch
    {
        "core" or "engine" => 1,
        "os" or "core/os" or "system" => 2,
        "hardware" => 3,
        "network" or "networking" => 4,
        "security" => 5,
        "software" => 6,
        "performance" => 7,
        "policy" or "gpo" or "policy/gpo" => 8,
        "startup" or "autoruns" => 9,
        "eventlog" or "audit" or "audit/eventlog" => 10,
        "export" or "exporters" => 11,
        "reader" or "readers" or "io" or "readers/io" => 12,
        "diagnostics" or "rules" or "etw" or "diagnostics/rules" => 13,
        _ => 0
    };
}

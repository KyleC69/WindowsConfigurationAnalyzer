using System.Text;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Export;

public sealed class HtmlReportBuilder : IExporter
{
    public async Task ExportAsync(AnalyzerResult result, string targetPath, CancellationToken cancellationToken)
    {
        var dir = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var html = BuildHtml(result);
        var tmp = targetPath + ".tmp";
        await File.WriteAllTextAsync(tmp, html, Encoding.UTF8, cancellationToken);
        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }

        File.Move(tmp, targetPath);
    }

    private static string BuildHtml(AnalyzerResult r)
    {
        var sb = new StringBuilder();
        sb.Append("<html><head><meta charset='utf-8'><title>WCA Report ");
        sb.Append(E(r.ComputerName));
        sb.Append("</title><style>body{font-family:Segoe UI,Arial,sans-serif;margin:20px}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ddd;padding:6px}th{background:#f4f4f4}.critical{color:#b00020}.warning{color:#c77f00}.ok{color:#157347}</style></head><body>");
        sb.Append("<h1>Windows Configuration Analyzer Report</h1>");
        sb.Append("<h2>System: ").Append(E(r.ComputerName)).Append(" | Exported: ").Append(E(r.ExportTimestampUtc.ToString("u"))).Append("</h2>");

        var crit = r.GlobalFindings.Count(f => f.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase));
        var warn = r.GlobalFindings.Count(f => f.Severity.Equals("Warning", StringComparison.OrdinalIgnoreCase));
        sb.Append("<section id='summary'><h3>Summary</h3><ul>");
        sb.Append("<li>Critical Findings: <span class='critical'>").Append(crit).Append("</span></li>");
        sb.Append("<li>Warnings: <span class='warning'>").Append(warn).Append("</span></li>");
        sb.Append("<li>Areas: ").Append(r.Areas.Count).Append("</li>");
        sb.Append("</ul></section>");

        foreach (var a in r.Areas)
        {
            sb.Append("<section><h3>").Append(E(a.Area)).Append("</h3>");
            if (a.Warnings.Count > 0)
            {
                sb.Append("<div class='warning'>Warnings: ").Append(a.Warnings.Count).Append("</div>");
            }
            if (a.Errors.Count > 0)
            {
                sb.Append("<div class='critical'>Errors: ").Append(a.Errors.Count).Append("</div>");
            }
            if (a.Anomalies.Count > 0)
            {
                sb.Append("<ul>");
                foreach (var f in a.Anomalies)
                {
                    var cls = CssFor(f.Severity);
                    sb.Append("<li class='").Append(cls).Append("'>").Append(E(f.Severity)).Append(": ").Append(E(f.Message)).Append("</li>");
                }
                sb.Append("</ul>");
            }
            // Render details as JSON for now for simplicity
            if (a.Details is not null)
            {
                sb.Append("<details><summary>Details</summary><pre>");
                var json = System.Text.Json.JsonSerializer.Serialize(a.Details, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                sb.Append(E(json));
                sb.Append("</pre></details>");
            }
            sb.Append("</section>");
        }

        if (r.ActionLog.Count > 0)
        {
            sb.Append("<section id='actionlog'><h3>Action Log</h3><table><tr><th>Time (UTC)</th><th>Area</th><th>Action</th><th>Level</th><th>Message</th></tr>");
            foreach (var e in r.ActionLog)
            {
                sb.Append("<tr><td>").Append(E(e.TimestampUtc.ToString("u"))).Append("</td>")
                .Append("<td>").Append(E(e.Area)).Append("</td>")
                .Append("<td>").Append(E(e.Action)).Append("</td>")
                .Append("<td>").Append(E(e.Level)).Append("</td>")
                .Append("<td>").Append(E(e.Message)).Append("</td></tr>");
            }
            sb.Append("</table></section>");
        }

        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static string CssFor(string severity) => severity.Equals("Critical", StringComparison.OrdinalIgnoreCase) ? "critical" : severity.Equals("Warning", StringComparison.OrdinalIgnoreCase) ? "warning" : "ok";
    private static string E(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
}

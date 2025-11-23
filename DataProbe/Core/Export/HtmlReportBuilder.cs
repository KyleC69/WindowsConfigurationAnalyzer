//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   HtmlReportBuilder.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Net;
using System.Text;
using System.Text.Json;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Export;


public sealed class HtmlReportBuilder : IExporter
{


    public async Task ExportAsync(AnalyzerResult result, string targetPath, CancellationToken cancellationToken)
    {
        string? dir = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string html = BuildHtml(result);
        string tmp = targetPath + ".tmp";
        await File.WriteAllTextAsync(tmp, html, Encoding.UTF8, cancellationToken);
        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }

        File.Move(tmp, targetPath);
    }





    private static string BuildHtml(AnalyzerResult r)
    {
        StringBuilder sb = new();
        sb.Append("<html><head><meta charset='utf-8'><title>WCA Report ");
        sb.Append(E(r.ComputerName));
        sb.Append(
            "</title><style>body{font-family:Segoe UI,Arial,sans-serif;margin:20px}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ddd;padding:6px}th{background:#f4f4f4}.critical{color:#b00020}.warning{color:#c77f00}.ok{color:#157347}</style></head><body>");
        sb.Append("<h1>Windows Configuration Analyzer Report</h1>");
        sb.Append("<h2>System: ").Append(E(r.ComputerName)).Append(" | Exported: ")
            .Append(E(r.ExportTimestampUtc.ToString("u"))).Append("</h2>");

        int crit = r.GlobalFindings.Count(f => f.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase));
        int warn = r.GlobalFindings.Count(f => f.Severity.Equals("Warning", StringComparison.OrdinalIgnoreCase));
        sb.Append("<section id='summary'><h3>Summary</h3><ul>");
        sb.Append("<li>Critical Findings: <span class='critical'>").Append(crit).Append("</span></li>");
        sb.Append("<li>Warnings: <span class='warning'>").Append(warn).Append("</span></li>");
        sb.Append("<li>Areas: ").Append(r.Areas.Count).Append("</li>");
        sb.Append("</ul></section>");

        foreach (AreaResult a in r.Areas)
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
                foreach (Finding f in a.Anomalies)
                {
                    string cls = CssFor(f.Severity);
                    sb.Append("<li class='").Append(cls).Append("'>").Append(E(f.Severity)).Append(": ")
                        .Append(E(f.Message)).Append("</li>");
                }

                sb.Append("</ul>");
            }

            // Render details as JSON for now for simplicity
            if (a.Details is not null)
            {
                sb.Append("<details><summary>Details</summary><pre>");
                string json = JsonSerializer.Serialize(a.Details, new JsonSerializerOptions { WriteIndented = true });
                sb.Append(E(json));
                sb.Append("</pre></details>");
            }

            sb.Append("</section>");
        }


        sb.Append("</body></html>");

        return sb.ToString();
    }





    private static string CssFor(string severity)
    {
        return severity.Equals("Critical", StringComparison.OrdinalIgnoreCase) ? "critical" :
            severity.Equals("Warning", StringComparison.OrdinalIgnoreCase) ? "warning" : "ok";
    }





    private static string E(string? s)
    {
        return WebUtility.HtmlEncode(s ?? string.Empty);
    }


}
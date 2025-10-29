using System.Text.Json;
using System.Text.Json.Serialization;
using WindowsConfigurationAnalyzer.Contracts;
using WindowsConfigurationAnalyzer.Models;

namespace WindowsConfigurationAnalyzer.Export;

public sealed class JsonExporter : IExporter
{
 private static readonly JsonSerializerOptions Options = new()
 {
 WriteIndented = true,
 DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
 };

 public async Task ExportAsync(AnalyzerResult result, string targetPath, CancellationToken cancellationToken)
 {
 var dir = Path.GetDirectoryName(targetPath);
 if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

 var tmp = targetPath + ".tmp";
 await using (var fs = File.Create(tmp))
 {
 await JsonSerializer.SerializeAsync(fs, result, Options, cancellationToken);
 await fs.FlushAsync(cancellationToken);
 }
 // Atomic move
 if (File.Exists(targetPath)) File.Delete(targetPath);
 File.Move(tmp, targetPath);
 }
}

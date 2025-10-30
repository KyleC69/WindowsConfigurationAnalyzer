using System.Text.Json;
using System.Text.Json.Serialization;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Diagnostics.Etw;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;

namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Export;

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
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var tmp = targetPath + ".tmp";
        await using (var fs = File.Create(tmp))
        {
            await JsonSerializer.SerializeAsync(fs, result, Options, cancellationToken);
            await fs.FlushAsync(cancellationToken);
        }
        // Atomic move
        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }

        File.Move(tmp, targetPath);

        // Emit ETW ExportCompleted (11501)
        WcaEventSource.Log.ExportCompleted(Guid.NewGuid().ToString("N"), "json", targetPath);
    }
}

//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   JsonExporter.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Text.Json;
using System.Text.Json.Serialization;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.Contracts.Models;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Export;


public sealed class JsonExporter : IExporter
{


    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };





    public async Task ExportAsync(AnalyzerResult result, string targetPath, CancellationToken cancellationToken)
    {
        string? dir = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string tmp = targetPath + ".tmp";
        await using (FileStream fs = File.Create(tmp))
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
    }


}
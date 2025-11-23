//  Created:  2025/11/16
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   Models.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;


public sealed record AnalyzerResult(
    string ComputerName,
    DateTimeOffset ExportTimestampUtc,
    IReadOnlyList<AreaResult> Areas,
    IReadOnlyList<Finding> GlobalFindings
);



public sealed record AreaResult(
    string Area,
    object? Summary,
    object? Details,
    IReadOnlyList<Finding> Anomalies,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors);



public sealed record ActionLogEntry(
    DateTimeOffset TimestampUtc,
    string Area,
    string Action,
    string Level,
    string Message,
    string? Exception);



public sealed record Finding(
    string Severity, // Info | Warning | Critical
    string Message,
    string? Context = null);
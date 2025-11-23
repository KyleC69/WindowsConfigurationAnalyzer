//  Created:  2025/11/16
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   Contracts.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Runtime.CompilerServices;

using KC.WindowsConfigurationAnalyzer.Contracts.Models;

#endregion





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IRule
{


    string Id { get; }

    string Area { get; }

    Finding? Evaluate(AnalyzerResult result);


}



// Reader contracts
public interface IRegistryReader
{


    object? GetValue(string hiveAndPath, string name);

    IEnumerable<string> EnumerateSubKeys(string hiveAndPath);

    IEnumerable<string> EnumerateValueNames(string hiveAndPath);


}



public interface ICimReader
{


    Task<IReadOnlyList<IDictionary<string, object?>>> QueryAsync(string wql, string? scope = null, CancellationToken cancellationToken = default, [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerPage = "");


}



public sealed record EventLogSummary(string LogName, int EntryCount, DateTimeOffset? LastWriteTimeUtc);



public interface IEventLogReader
{


    EventLogSummary? GetSummary(string logName);


}



public interface IFirewallReader
{


    IEnumerable<string> GetProfiles();

    IEnumerable<object> GetRules();


}



public interface IEnvReader
{


    string MachineName { get; }

    string OSVersionString { get; }

    bool Is64BitOS { get; }

    string UserName { get; }

    string UserDomainName { get; }

    IReadOnlyDictionary<string, string?> GetEnvironmentVariables();


}



// Manifest-based ETW Provider Abstraction (stubs to be implemented by runtime)
public interface IEventProvider
{


    // Emits a general action event mapped by taxonomy. Sequence helps keep IDs within area blocks.
    void EmitAction(string area, string action, string level, string message, string? exception, int sequence);





    // Session lifecycle helpers (map to1001/1002 per manifest)
    void EmitSessionStart(string sessionId, string computer, string version);

    void EmitSessionStop(string sessionId, int areas, int warnings, int errors, double elapsedSeconds);





    // Export completed helper (maps to11501)
    void EmitExportCompleted(string sessionId, string format, string path);


}
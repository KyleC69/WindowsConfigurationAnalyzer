//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   EventLogReader.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.Diagnostics;
using System.Security;

using KC.WindowsConfigurationAnalyzer.Contracts;





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public sealed class EventLogReader : IEventLogReader
{


    public EventLogSummary? GetSummary(string logName)
    {
        try
        {
            using EventLog ev = new(logName);

            if (ev.Entries == null || ev.Entries.Count == 0) return new EventLogSummary(logName, 0, null);

            var lastIndex = ev.Entries.Count - 1;
            EventLogEntry? last = ev.Entries[lastIndex];
            DateTime lastUtc = DateTime.SpecifyKind(last.TimeGenerated, DateTimeKind.Local).ToUniversalTime();

            return new EventLogSummary(logName, ev.Entries.Count, lastUtc);
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (SecurityException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }


}
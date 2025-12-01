//  Created:  2025/11/24
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   IEventProvider.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts;


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
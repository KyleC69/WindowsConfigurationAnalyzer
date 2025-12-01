//  Created:  2025/11/24
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   ICimReader.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.Runtime.CompilerServices;





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface ICimReader
{


    Task<IReadOnlyList<IDictionary<string, object?>>> QueryAsync(string wql, string? scope = null, CancellationToken cancellationToken = default, [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerPage = "");


}
//  Created:  2025/11/24
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   IEnvReader.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IEnvReader
{


    string MachineName { get; }

    string OSVersionString { get; }

    bool Is64BitOS { get; }

    string UserName { get; }

    string UserDomainName { get; }

    IReadOnlyDictionary<string, string?> GetEnvironmentVariables();


}
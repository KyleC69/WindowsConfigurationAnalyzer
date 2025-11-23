//  Created:  2025/11/16
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   IExporter.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts.Models;

#endregion





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IExporter
{


    Task ExportAsync(AnalyzerResult result, string targetPath, CancellationToken cancellationToken);


}
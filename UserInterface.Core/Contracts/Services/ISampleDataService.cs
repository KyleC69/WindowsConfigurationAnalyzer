//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface.Core
//        File:   ISampleDataService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;


// Remove this class once your pages/features are using your data.
public interface ISampleDataService
{


    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();


}
//  Created:  2025/11/16
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   IPageService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;


public interface IPageService
{


    Type GetPageType(string key);


}
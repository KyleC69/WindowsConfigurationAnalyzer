//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   INavigationService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface INavigationService
{


    bool CanGoBack { get; }

    Frame? Frame { get; set; }

    event NavigatedEventHandler Navigated;

    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

    bool GoBack();


}
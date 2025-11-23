//  Created:  2025/11/16
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   IAppNotificationService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Collections.Specialized;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;


public interface IAppNotificationService
{


    void Initialize();

    bool Show(string payload);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();


}
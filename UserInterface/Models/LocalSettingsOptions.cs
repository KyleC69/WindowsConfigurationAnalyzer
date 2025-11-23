//  Created:  2025/11/09
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   LocalSettingsOptions.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.UserInterface.Models;


public class LocalSettingsOptions
{


    public string? ApplicationDataFolder { get; set; }

    public string? LocalSettingsFile { get; set; }


}
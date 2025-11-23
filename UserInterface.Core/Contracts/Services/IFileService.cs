//  Created:  2025/11/16
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface.Core
//        File:   IFileService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;


public interface IFileService
{


    T? Read<T>(string folderPath, string fileName);

    void Save<T>(string folderPath, string fileName, T content);

    void Delete(string folderPath, string fileName);


}
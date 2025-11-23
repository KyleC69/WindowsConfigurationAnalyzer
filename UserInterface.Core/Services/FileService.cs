//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface.Core
//        File:   FileService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Text;

using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;

using Newtonsoft.Json;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Services;


public class FileService : IFileService
{


    public T? Read<T>(string folderPath, string fileName)
    {
        string path = Path.Combine(folderPath, fileName);

        if (!File.Exists(path))
        {
            return default;
        }

        string json = File.ReadAllText(path);

        return JsonConvert.DeserializeObject<T>(json);
    }





    public void Save<T>(string folderPath, string fileName, T content)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileContent = JsonConvert.SerializeObject(content);
        File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
    }





    public void Delete(string folderPath, string fileName)
    {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
        {
            File.Delete(Path.Combine(folderPath, fileName));
        }
    }


}
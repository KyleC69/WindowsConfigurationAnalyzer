//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   ILocalSettingsService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;


public class StorageFile
{


    public StorageFile(string path)
    {
        Path = path;
    }





    public string Path { get; }


}



public interface ILocalSettingsService
{


    Task SaveDataAsync<T>(string fileName, T data);

    Task<T?> ReadDataAsync<T>(string filename);

    Task SaveObjectAsync<T>(string key, T obj);

    Task SaveApplicationSettingAsync(string key, string value);

    Task<T?> ReadApplicationSettingAsync<T>(string key);

    Task<StorageFile> SaveBinaryFileAsync(string fileName, byte[] data);

    Task<byte[]?> ReadBinaryFileAsync(string fileName);

    Task<byte[]?> ReadBytesFromFileAsync(StorageFile file);


}
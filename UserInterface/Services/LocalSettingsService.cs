//  Created:  2025/11/09
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   LocalSettingsService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers;

using Microsoft.Win32;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Services;


public class LocalSettingsService : ILocalSettingsService
{


    private const string RegistryCompany = "KC";
    private const string RegistryProduct = "WindowsConfigurationAnalyzer";
    private readonly string _localFolderPath;
    private bool _isInitialized;





    public LocalSettingsService()
    {
        _localFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            RegistryCompany, RegistryProduct);
        Directory.CreateDirectory(_localFolderPath);
    }





    public async Task SaveDataAsync<T>(string fileName, T data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));

        string fullPath = Path.Combine(_localFolderPath, fileName + ".json");
        string json = await Json.StringifyAsync(data);
        await File.WriteAllTextAsync(fullPath, json);
    }





    public async Task<T?> ReadDataAsync<T>(string filename)
    {
        string fullPath = Path.Combine(_localFolderPath, filename + ".json");

        if (!File.Exists(fullPath)) return default;

        string json = await File.ReadAllTextAsync(fullPath);

        return await Json.ToObjectAsync<T>(json);
    }





    public async Task SaveObjectAsync<T>(string key, T obj)
    {
        await SaveDataAsync(key, obj);
    }





    public Task SaveApplicationSettingAsync(string key, string value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));

        if (value is null) throw new ArgumentNullException(nameof(value));

        using RegistryKey? k = OpenRegistryKey(true);
        k?.SetValue(key, value, RegistryValueKind.String);

        return Task.CompletedTask;
    }





    public async Task<T?> ReadApplicationSettingAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

        using RegistryKey? k = OpenRegistryKey(false);
        string? raw = k?.GetValue(key) as string;

        if (string.IsNullOrEmpty(raw)) return default;

        try
        {
            return await Json.ToObjectAsync<T>(raw);
        }
        catch
        {
            return typeof(T) == typeof(string) ? (T?)(object?)raw : default;
        }
    }





    public async Task<StorageFile> SaveBinaryFileAsync(string fileName, byte[] data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));

        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Invalid file name", nameof(fileName));

        string fullPath = Path.Combine(_localFolderPath, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, data);

        return new StorageFile(fullPath);
    }





    public async Task<byte[]?> ReadBinaryFileAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

        string fullPath = Path.Combine(_localFolderPath, fileName);

        return !File.Exists(fullPath) ? null : await File.ReadAllBytesAsync(fullPath);
    }





    public async Task<byte[]?> ReadBytesFromFileAsync(StorageFile file)
    {
        return file == null || string.IsNullOrEmpty(file.Path) || !File.Exists(file.Path) ? null : await File.ReadAllBytesAsync(file.Path);
    }





    private async Task InitializeAsync()
    {
        if (_isInitialized) return;

        _isInitialized = true;
        await Task.CompletedTask;
    }





    private async Task PersistModelAsync()
    {
        await Task.CompletedTask;
    }





    private RegistryKey? OpenRegistryKey(bool writable)
    {
        string subKey = $"Software\\{RegistryCompany}\\{RegistryProduct}";

        return writable
            ? Registry.CurrentUser.CreateSubKey(subKey, true)
            : Registry.CurrentUser.OpenSubKey(subKey, false);
    }


}
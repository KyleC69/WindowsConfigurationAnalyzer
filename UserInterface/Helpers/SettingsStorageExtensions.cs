//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   SettingsStorageExtensions.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Runtime.Versioning;

using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers;

using Microsoft.Win32;

using Windows.Storage;
using Windows.Storage.Streams;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


/// <summary>
///     Provides extension methods for storing and retrieving local and roaming application data.
/// </summary>
/// <remarks>
///     This class includes methods for saving and reading data in JSON format, handling both local and roaming storage.
///     It also provides utilities for working with files and application settings containers.
///     For more details, refer to the documentation on storing and retrieving app data at:
///     https://docs.microsoft.com/windows/apps/design/app-settings/store-and-retrieve-app-data
/// </remarks>
[SupportedOSPlatform("windows10.0.22601.0")]
public static class SettingsStorageExtensions
{


    private const string FileExtension = ".json";

    private const string RegistryCompany = "KC";
    private const string RegistryProduct = "WindowsConfigurationAnalyzer";





    /// <summary>
    ///     Determines whether roaming storage is available for the current application.
    /// </summary>
    /// <param name="appData">The <see cref="ApplicationData" /> instance representing the application's data store.</param>
    /// <returns>
    ///     <c>true</c> if roaming storage is available; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Roaming storage allows data to be synchronized across devices for the same user.
    ///     This method checks the availability of roaming storage by verifying the
    ///     <see cref="ApplicationData.RoamingStorageQuota" /> value.
    /// </remarks>
    public static bool IsRoamingStorageAvailable(this ApplicationData appData)
    {
        // Use presence of the user roaming AppData folder as an indicator
        string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        return !string.IsNullOrWhiteSpace(roamingPath) && Directory.Exists(roamingPath);
    }





    /// <summary>
    ///     Saves the specified content to a file in the given storage folder asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the content to save. The content will be serialized to JSON format.</typeparam>
    /// <param name="folder">The <see cref="StorageFolder" /> where the file will be saved.</param>
    /// <param name="name">The name of the file (without extension) to save the content to.</param>
    /// <param name="content">The content to save. Must not be <c>null</c>.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="content" /> is <c>null</c>.</exception>
    /// <remarks>
    ///     The file will be created or replaced if it already exists. The content is serialized to JSON format
    ///     before being written to the file. The file name is automatically suffixed with a predefined extension.
    /// </remarks>
    public static async Task SaveAsync<T>(this StorageFolder folder, string name, T content)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));

        string fullPath = Path.Combine(folder.Path, GetFileName(name));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        string fileContent = await Json.StringifyAsync(content);

        await File.WriteAllTextAsync(fullPath, fileContent);
    }





    /// <summary>
    ///     Reads and deserializes a JSON file from the specified <see cref="StorageFolder" /> into an object of type
    ///     <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize the JSON content into.</typeparam>
    /// <param name="folder">The <see cref="StorageFolder" /> containing the file to read.</param>
    /// <param name="name">The name of the file (without extension) to read and deserialize.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the deserialized object of type
    ///     <typeparamref name="T" />
    ///     if the file exists and is successfully read; otherwise, the default value of <typeparamref name="T" />.
    /// </returns>
    /// <remarks>
    ///     This method checks if the specified file exists in the folder. If the file is found, it reads its content,
    ///     deserializes it from JSON format, and returns the resulting object. If the file does not exist, it returns the
    ///     default value of <typeparamref name="T" />.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the JSON content cannot be deserialized into the specified type
    ///     <typeparamref name="T" />.
    /// </exception>
    /// <example>
    ///     The following example demonstrates how to use the <c>ReadAsync</c> method:
    ///     <code>
    /// StorageFolder folder = ApplicationData.Current.LocalFolder;
    /// var settings = await folder.ReadAsync<MySettings>("settings");
    /// </code>
    /// </example>
    public static async Task<T?> ReadAsync<T>(this StorageFolder folder, string name)
    {
        string fullPath = Path.Combine(folder.Path, GetFileName(name));

        if (!File.Exists(fullPath)) return default;

        string fileContent = await File.ReadAllTextAsync(fullPath);

        return await Json.ToObjectAsync<T>(fileContent);
    }





    /// <summary>
    ///     Saves a value to the specified <see cref="ApplicationDataContainer" /> using the provided key.
    /// </summary>
    /// <typeparam name="T">The type of the value to be saved. The value will be serialized to JSON format.</typeparam>
    /// <param name="settings">The <see cref="ApplicationDataContainer" /> where the value will be stored.</param>
    /// <param name="key">The key under which the value will be saved.</param>
    /// <param name="value">The value to save. Must not be <c>null</c>.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value" /> is <c>null</c>.</exception>
    /// <remarks>
    ///     This method serializes the value to JSON format before saving it to the <paramref name="settings" /> container.
    ///     It is useful for storing complex objects in application settings.
    /// </remarks>
    public static async Task SaveAsync<T>(this ApplicationDataContainer settings, string key, T value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        string str = await Json.StringifyAsync(value);
        SaveString(settings, key, str);
    }





    /// <summary>
    ///     Saves a string value to the specified <see cref="ApplicationDataContainer" /> using the provided key.
    /// </summary>
    /// <param name="settings">The <see cref="ApplicationDataContainer" /> where the value will be stored.</param>
    /// <param name="key">The key under which the value will be stored.</param>
    /// <param name="value">The string value to save.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the <paramref name="value" /> is <c>null</c>.
    /// </exception>
    /// <remarks>
    ///     This method stores the value in the <see cref="ApplicationDataContainer.Values" /> dictionary.
    ///     If a value with the same key already exists, it will be replaced.
    /// </remarks>
    public static void SaveString(this ApplicationDataContainer settings, string key, string value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        using RegistryKey? appKey = GetAppRegistryKey(true);
        appKey?.SetValue(key, value, RegistryValueKind.String);
    }





    /// <summary>
    ///     Reads a value of the specified type from the application data container using the provided key.
    /// </summary>
    /// <typeparam name="T">The type of the value to be read.</typeparam>
    /// <param name="settings">The <see cref="ApplicationDataContainer" /> from which the value will be read.</param>
    /// <param name="key">The key associated with the value to be read.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the value of type
    ///     <typeparamref name="T" />
    ///     if the key exists and the value can be deserialized; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    ///     This method attempts to retrieve a value stored as a string in the specified application data container.
    ///     If the value exists and is not empty, it is deserialized into the specified type <typeparamref name="T" />.
    ///     If the key does not exist or the value cannot be deserialized, the method returns <c>null</c>.
    /// </remarks>
    /// <example>
    ///     The following example demonstrates how to use the <see cref="ReadAsync{T}" /> method:
    ///     <code>
    /// var container = ApplicationData.Current.LocalSettings;
    /// var myValue = await container.ReadAsync<int>("MyKey");
    /// </code>
    /// </example>
    public static async Task<T?> ReadAsync<T>(this ApplicationDataContainer settings, string key)
    {
        using RegistryKey? appKey = GetAppRegistryKey(false);
        string? obj = appKey?.GetValue(key) as string;

        return !string.IsNullOrEmpty(obj) ? await Json.ToObjectAsync<T>(obj) : default;
    }





    /// <summary>
    ///     Saves the specified byte array content to a file in the given storage folder.
    /// </summary>
    /// <param name="folder">The <see cref="StorageFolder" /> where the file will be saved.</param>
    /// <param name="content">The byte array content to save to the file.</param>
    /// <param name="fileName">The name of the file to create or overwrite.</param>
    /// <param name="options">
    ///     Specifies the <see cref="CreationCollisionOption" /> to determine the behavior if a file with the same name already
    ///     exists.
    ///     Defaults to <see cref="CreationCollisionOption.ReplaceExisting" />.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the created <see cref="StorageFile" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the <paramref name="content" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the <paramref name="fileName" /> parameter is <c>null</c> or an empty string.
    /// </exception>
    public static async Task<StorageFile> SaveFileAsync(this StorageFolder folder, byte[] content, string fileName,
        CreationCollisionOption options =
            CreationCollisionOption.ReplaceExisting)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("Log filename is null or empty. Specify a valid file name.");

        string fullPath = Path.Combine(folder.Path, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        bool fileExists = File.Exists(fullPath);

        if (fileExists && options == CreationCollisionOption.FailIfExists) throw new IOException($"File already exists: {fullPath}");

        if (!fileExists || options == CreationCollisionOption.ReplaceExisting) await File.WriteAllBytesAsync(fullPath, content);

        // Wrap back into a StorageFile for compatibility with existing callers
        StorageFile? storageFile = await StorageFile.GetFileFromPathAsync(fullPath);

        return storageFile;
    }





    /// <summary>
    ///     Reads the content of a file as a byte array asynchronously.
    /// </summary>
    /// <param name="folder">The <see cref="StorageFolder" /> containing the file to read.</param>
    /// <param name="fileName">The name of the file to read.</param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation. The result contains the file content as a byte
    ///     array,
    ///     or <c>null</c> if the file does not exist or is not of type <see cref="StorageItemTypes.File" />.
    /// </returns>
    /// <remarks>
    ///     This method attempts to locate the specified file in the provided folder. If the file exists and is of the correct
    ///     type,
    ///     its content is read and returned as a byte array. If the file does not exist or is not a file, the method returns
    ///     <c>null</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="folder" /> or <paramref name="fileName" /> is
    ///     <c>null</c>.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the app does not have permission to access the file.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the file is not found in the specified folder.</exception>
    public static async Task<byte[]?> ReadFileAsync(this StorageFolder folder, string fileName)
    {
        string fullPath = Path.Combine(folder.Path, fileName);
        if (File.Exists(fullPath))
        {
            byte[] content = await File.ReadAllBytesAsync(fullPath);

            return content;
        }

        return null;
    }





    /// <summary>
    ///     Reads the contents of the specified <see cref="StorageFile" /> as a byte array asynchronously.
    /// </summary>
    /// <param name="file">The <see cref="StorageFile" /> to read from.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the byte array
    ///     with the file's contents, or <c>null</c> if the file is <c>null</c>.
    /// </returns>
    /// <remarks>
    ///     This method opens the file for reading, reads its contents into a byte array, and then closes the file.
    ///     It uses <see cref="IRandomAccessStream" /> and <see cref="DataReader" /> to perform the read operation.
    /// </remarks>
    /// <exception cref="Exception">
    ///     An exception may be thrown if the file cannot be opened or read due to issues such as file access permissions
    ///     or file corruption.
    /// </exception>
    public static async Task<byte[]?> ReadBytesAsync(this StorageFile file)
    {
        if (file != null)
        {
            string path = file.Path;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                byte[] bytes = await File.ReadAllBytesAsync(path);

                return bytes;
            }
        }

        return null;
    }





    /// <summary>
    ///     Generates a file name by appending a predefined file extension to the specified name.
    /// </summary>
    /// <param name="name">The base name of the file.</param>
    /// <returns>The generated file name with the predefined file extension appended.</returns>
    /// <remarks>
    ///     This method is used internally to standardize file naming conventions for storage operations.
    ///     The file extension is defined as a constant within the class.
    /// </remarks>
    private static string GetFileName(string name)
    {
        return string.Concat(name, FileExtension);
    }





    private static RegistryKey? GetAppRegistryKey(bool writable)
    {
        string subKey = $"Software\\{RegistryCompany}\\{RegistryProduct}";

        return writable
            ? Registry.CurrentUser.CreateSubKey(subKey, true)
            : Registry.CurrentUser.OpenSubKey(subKey, false);
    }


}
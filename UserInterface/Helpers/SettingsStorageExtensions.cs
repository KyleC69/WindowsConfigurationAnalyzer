// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  SettingsStorageExtensions.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;



// Use these extension methods to store and retrieve local and roaming app data
// More details regarding storing and retrieving app data at https://docs.microsoft.com/windows/apps/design/app-settings/store-and-retrieve-app-data
public static class SettingsStorageExtensions
{
    private const string FileExtension = ".json";





    public static bool IsRoamingStorageAvailable(this ApplicationData appData)
    {
        return appData.RoamingStorageQuota == 0;
    }





    public static async Task SaveAsync<T>(this StorageFolder folder, string name, T content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        StorageFile? file = await folder.CreateFileAsync(GetFileName(name), CreationCollisionOption.ReplaceExisting);
        var fileContent = await Json.StringifyAsync(content);

        await FileIO.WriteTextAsync(file, fileContent);
    }





    public static async Task<T?> ReadAsync<T>(this StorageFolder folder, string name)
    {
        if (!File.Exists(Path.Combine(folder.Path, GetFileName(name))))
        {
            return default(T?);
        }

        StorageFile? file = await folder.GetFileAsync($"{name}.json");
        var fileContent = await FileIO.ReadTextAsync(file);

        return await Json.ToObjectAsync<T>(fileContent);
    }





    public static async Task SaveAsync<T>(this ApplicationDataContainer settings, string key, T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        settings.SaveString(key, await Json.StringifyAsync(value));
    }





    public static void SaveString(this ApplicationDataContainer settings, string key, string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        settings.Values[key] = value;
    }





    public static async Task<T?> ReadAsync<T>(this ApplicationDataContainer settings, string key)
    {

        if (settings.Values.TryGetValue(key, out var obj) && obj is string s && !string.IsNullOrEmpty(s))
        {
            return await Json.ToObjectAsync<T>(s);
        }

        return default(T?);
    }





    public static async Task<StorageFile> SaveFileAsync(this StorageFolder folder, byte[] content, string fileName,
                                                        CreationCollisionOption options =
                                                            CreationCollisionOption.ReplaceExisting)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException(
                ResourceManager.Current.MainResourceMap
                    .GetValue(
                        "UserInterface/Resources/SettingsStorageExtensions_SaveFileAsync_File_name_is_null_or_empty__Specify_a_valid_file_name")
                    .ValueAsString, nameof(fileName));
        }

        StorageFile? storageFile = await folder.CreateFileAsync(fileName, options);
        await FileIO.WriteBytesAsync(storageFile, content);

        return storageFile;
    }





    public static async Task<byte[]?> ReadFileAsync(this StorageFolder folder, string fileName)
    {
        IStorageItem? item = await folder.TryGetItemAsync(fileName).AsTask().ConfigureAwait(false);

        if (item != null && item.IsOfType(StorageItemTypes.File))
        {
            StorageFile? storageFile = await folder.GetFileAsync(fileName);
            var content = await storageFile.ReadBytesAsync();

            return content;
        }

        return null;
    }





    public static async Task<byte[]?> ReadBytesAsync(this StorageFile file)
    {
        if (file != null)
        {
            using IRandomAccessStream stream = await file.OpenReadAsync();
            using DataReader reader = new(stream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)stream.Size);
            var bytes = new byte[stream.Size];
            reader.ReadBytes(bytes);

            return bytes;
        }

        return null;
    }





    private static string GetFileName(string name)
    {
        return string.Concat(name, FileExtension);
    }
}
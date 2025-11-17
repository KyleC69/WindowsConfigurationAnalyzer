// Created:  2025/11/10
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  ILocalSettingsService.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




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
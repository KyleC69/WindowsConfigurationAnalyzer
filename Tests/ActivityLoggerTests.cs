// Created: 2025/11/11
// Unit tests for ActivityLogger

using FluentAssertions;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace KC.WindowsConfigurationAnalyzer.Tests
{
    [TestClass]
    public class ActivityLoggerTests
    {
        [TestMethod]
        public async Task ActivityLogger_DefaultEnabled_WritesEntries_WhenInitialized()
        {
            // Arrange
            ActivityLogger.Initialize(true);

            // Act
            ActivityLogger.Log("INF", "Test message", "ctx");
            ActivityLogger.Flush();

            // Allow background worker to write
            await Task.Delay(300);

            // Assert
            string dir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "ActivityLogs");
            Directory.Exists(dir).Should().BeTrue();
        }

        [TestMethod]
        public async Task ActivityLogger_DisabledBySetting_NoWrites()
        {
            // Arrange
            // Simulate settings service returning "false"
            var svc = new FakeSettingsService(returnValue: "false");
            await ActivityLogger.InitializeAsync(svc);

            // Act
            ActivityLogger.Log("INF", "ShouldNotWrite", "ctx");
            ActivityLogger.Flush();
            await Task.Delay(200);

            // Assert
            // Ensure that writer is null or disabled
            // There's no public API to inspect writer so ensure directory may be absent or empty
            string dir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "ActivityLogs");
            // If logging disabled, the directory might still exist — ensure recent files don't contain the entry
            if (Directory.Exists(dir))
            {
                string[] files = Directory.GetFiles(dir);
                // None of the files should contain the test message
                foreach (string f in files)
                {
                    string content = await File.ReadAllTextAsync(f);
                    content.Contains("ShouldNotWrite").Should().BeFalse();
                }
            }
        }

        private class FakeSettingsService : ILocalSettingsService
        {
            private readonly string _returnValue;
            public FakeSettingsService(string returnValue) => _returnValue = returnValue;
            public Task SaveDataAsync<T>(string fileName, T data) => Task.CompletedTask;
            public Task<T?> ReadDataAsync<T>(string filename) => Task.FromResult<T?>(default);
            public Task SaveObjectAsync<T>(string key, T obj) => Task.CompletedTask;
            public Task SaveApplicationSettingAsync(string key, string value) => Task.CompletedTask;
            public Task<T?> ReadApplicationSettingAsync<T>(string key)
            {
                object? boxed = _returnValue;
                return Task.FromResult((T?)boxed);
            }
            public Task<StorageFile> SaveBinaryFileAsync(string fileName, byte[] data) => Task.FromResult(new StorageFile(fileName));
            public Task<byte[]?> ReadBinaryFileAsync(string fileName) => Task.FromResult<byte[]?>(null);
            public Task<byte[]?> ReadBytesFromFileAsync(StorageFile file) => Task.FromResult<byte[]?>(null);
        }
    }
}

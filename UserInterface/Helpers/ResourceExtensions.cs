using Microsoft.Windows.ApplicationModel.Resources;

namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

public static class ResourceExtensions
{
    private static readonly ResourceLoader _resourceLoader = new();

    public static string GetLocalized(this string resourceKey) => _resourceLoader.GetString(resourceKey);
}

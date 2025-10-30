namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Utilities;

public static class DictionaryExtensions
{
    public static object? GetOrDefault(this IDictionary<string, object?> dict, string key)
    => dict.TryGetValue(key, out var v) ? v : null;

    public static T? GetAs<T>(this IDictionary<string, object?> dict, string key)
    {
        if (dict.TryGetValue(key, out var v) && v is T t)
        {
            return t;
        }

        return default;
    }
}

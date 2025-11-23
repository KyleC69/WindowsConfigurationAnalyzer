//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface.Core
//        File:   Json.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Globalization;

using Newtonsoft.Json;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Helpers;


public static class Json
{


    public static async Task<T?> ToObjectAsync<T>(string value)
    {
        if (value is null)
        {
            return default;
        }

        // Fast-path for string targets: value is already the desired result
        if (typeof(T) == typeof(string))
        {
            return (T)(object)value;
        }

        string trimmed = value.Trim();

        // If target is an enum, try parsing directly from the token (common case like "Dark")
        if (typeof(T).IsEnum)
        {
            if (Enum.TryParse(typeof(T), trimmed, true, out object? enumVal))
            {
                return (T)enumVal;
            }
        }

        // Detect whether the input already looks like JSON. If not, quote it so Json.NET can parse it as a string.
        bool looksLikeJson = trimmed.StartsWith("{") || trimmed.StartsWith("[") || trimmed.StartsWith("\"")
                            || string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(trimmed, "null", StringComparison.OrdinalIgnoreCase)
                            || double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out _);

        string jsonInput = value;
        if (!looksLikeJson)
        {
            jsonInput = '"' + trimmed.Replace("\"", "\\\"") + '"';
        }

        try
        {
            // Keep asynchronous signature; offload deserialization to thread pool for heavier types
            return await Task.Run(() => JsonConvert.DeserializeObject<T>(jsonInput));
        }
        catch (JsonReaderException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize value as {typeof(T).Name}: {value}", ex);
        }
    }





    public static async Task<string> StringifyAsync(object value)
    {
        return await Task.Run<string>(() => JsonConvert.SerializeObject(value));
    }


}
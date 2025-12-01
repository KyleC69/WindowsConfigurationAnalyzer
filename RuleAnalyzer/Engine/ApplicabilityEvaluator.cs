//  Created:  2025/11/24
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   ApplicabilityEvaluator.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.Text.Json;





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


public class ApplicabilityEvaluator
{


    public bool IsApplicable(JsonElement applicability)
    {
        // Ensure the element is an object
        if (applicability.ValueKind != JsonValueKind.Object) return false;

        var osFamily = TryGetString(applicability, "OSFamily");
        var minVersionStr = TryGetString(applicability, "MinVersion");
        var maxVersionStr = TryGetString(applicability, "MaxVersion");
        var product = TryGetString(applicability, "Product");

        if (string.IsNullOrWhiteSpace(osFamily)) return false; // OSFamily is required for applicability

        Version currentVersion = Environment.OSVersion.Version;
        Version? minVersion = ParseVersion(minVersionStr);
        Version? maxVersion = ParseVersion(maxVersionStr);

        var currentPlatform = Environment.OSVersion.Platform.ToString();

        var osMatch = MatchesOsFamily(currentPlatform, osFamily);
        var versionInRange = (minVersion is null || currentVersion >= minVersion) &&
                             (maxVersion is null || currentVersion <= maxVersion);
        var productMatch = string.IsNullOrWhiteSpace(product) || MatchesProduct(product);

        return osMatch && versionInRange && productMatch;
    }





    private static string? TryGetString(JsonElement obj, string propertyName)
    {
        return obj.TryGetProperty(propertyName, out JsonElement prop) && prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
    }





    private static Version? ParseVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version)) return null;

        if (Version.TryParse(version, out Version? v)) return v;

        return null; // Ignore unparsable version
    }





    private static bool MatchesOsFamily(string platform, string osFamily)
    {
        // Normalize simple Windows mapping (Win32NT, Windows, etc.)
        return osFamily.Equals("Windows", StringComparison.OrdinalIgnoreCase) ? platform.Contains("Win", StringComparison.OrdinalIgnoreCase) : platform.IndexOf(osFamily, StringComparison.OrdinalIgnoreCase) >= 0;
    }





    private static bool MatchesProduct(string product)
    {
        // Placeholder for future product matching logic; accept any non-empty for now
        return true;
    }


}
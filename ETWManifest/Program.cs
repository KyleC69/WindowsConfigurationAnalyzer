//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Support.GenerateETWManifest
//        File:   Program.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Diagnostics.Tracing;
using System.Globalization;

using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Etw;

#endregion





// Generate the manifest
namespace KC.WindowsConfigurationAnalyzer.Support.GenerateETWManifest;


/// <summary>
///     Provides the application entry point for generating and saving an event manifest file based on the specified
///     command-line arguments.
/// </summary>
/// <remarks>
///     This class is intended for internal use and is not designed to be accessed directly by external
///     components. The application expects the '--outfile' argument followed by a valid file path to determine where the
///     generated manifest will be saved. The manifest is generated using the en-US culture to ensure consistency across
///     different operating system locales.
/// </remarks>
internal class Program
{


    private static void Main(string[] args)
    {
        string? outFilename = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] != "--outfile" || i + 1 >= args.Length)
            {
                continue;
            }

            outFilename = args[i + 1];

            break;
        }

        // Validate the filename
        if (string.IsNullOrEmpty(outFilename))
        {
            throw new ArgumentException("Missing or invalid --outfile argument");
        }

        // This ensures the generates manifest uses the default en-US culture when the build process runs on a non-US operating system
        CultureInfo cultureInfo = new("en-US");
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;

        // Generate the manifest
        string? manifest = EventSource.GenerateManifest(typeof(WCAEventSource), "WCA.Events.dll");

        // Save the manifest to a file
        if (manifest is not null)
        {
            File.WriteAllText(Path.GetFullPath(outFilename), manifest);
            Console.WriteLine($"Manifest generated and saved to {Path.GetFullPath(outFilename)}");
        }
        else
        {
            Console.WriteLine("Failed to generate manifest. The manifest content is null.");
        }
    }


}
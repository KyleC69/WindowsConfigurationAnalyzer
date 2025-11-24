//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   ResourceExtensions.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using Microsoft.Windows.ApplicationModel.Resources;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


public static class ResourceExtensions
{


    private static readonly ResourceLoader ResourceLoader = new();





    public static string GetLocalized(this string resourceKey)
    {
        return ResourceLoader.GetString(resourceKey) ?? $@"Localized resource not found for key:{resourceKey}";
    }


}
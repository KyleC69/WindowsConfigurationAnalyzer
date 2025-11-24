//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   DictionaryExtensions.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;


public static class DictionaryExtensions
{


    public static object? GetOrDefault(this IDictionary<string, object?> dict, string key)
    {
        return dict.TryGetValue(key, out object? v) ? v : null;
    }





    public static T? GetAs<T>(this IDictionary<string, object?> dict, string key)
    {
        return dict.TryGetValue(key, out object? v) && v is T t ? t : default;
    }


}
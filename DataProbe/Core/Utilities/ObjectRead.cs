//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   ObjectRead.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Reflection;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Utilities;


public static class ObjectRead
{


    public static bool TryGetProperty(object? obj, string name, out object? value)
    {
        value = null;

        if (obj is null)
        {
            return false;
        }

        if (obj is IDictionary<string, object?> dict)
        {
            return dict.TryGetValue(name, out value);
        }

        Type type = obj.GetType();
        PropertyInfo? prop =
            type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (prop is null)
        {
            return false;
        }

        value = prop.GetValue(obj);

        return true;
    }





    public static T? GetPropertyAs<T>(object? obj, string name)
    {
        if (TryGetProperty(obj, name, out object? v) && v is T t)
        {
            return t;
        }

        try
        {
            // Handle numeric conversions where possible
            if (TryGetProperty(obj, name, out object? v2) && v2 is not null)
            {
                return (T)Convert.ChangeType(v2, typeof(T));
            }
        }
        catch
        {
        }

        return default;
    }


}
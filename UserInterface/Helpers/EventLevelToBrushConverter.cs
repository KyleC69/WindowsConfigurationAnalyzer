//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   EventLevelToBrushConverter.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


public sealed class EventLevelToBrushConverter : IValueConverter
{


    public object Convert(object value, Type targetType, object parameter, string language)
    {
        byte level = 0;
        try
        {
            if (value is byte b)
                level = b;
            else if (value is sbyte sb)
                level = (byte)sb;
            else if (value is short s)
                level = (byte)s;
            else if (value is int i)
                level = (byte)i;
            else if (value is long l)
                level = (byte)l;
            else if (value is string str && byte.TryParse(str, out var parsed)) level = parsed;
        }
        catch
        {
            level = 0;
        }

        return level switch
        {
            1 => new SolidColorBrush(Colors.DarkRed), // Critical
            2 => new SolidColorBrush(Colors.Red), // Error
            3 => new SolidColorBrush(Colors.Orange), // Warning
            4 => new SolidColorBrush(Colors.CornflowerBlue), // Information
            5 => new SolidColorBrush(Colors.Gray), // Verbose
            _ => new SolidColorBrush(Colors.Gray)
        };
    }





    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }


}
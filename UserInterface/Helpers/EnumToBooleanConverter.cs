//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   EnumToBooleanConverter.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


public class EnumToBooleanConverter : IValueConverter
{


    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            if (!Enum.IsDefined(typeof(ElementTheme), value)) throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");

            object enumValue = Enum.Parse(typeof(ElementTheme), enumString);

            return enumValue.Equals(value);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }





    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return parameter is string enumString
            ? Enum.Parse(typeof(ElementTheme), enumString)
            : throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }


}
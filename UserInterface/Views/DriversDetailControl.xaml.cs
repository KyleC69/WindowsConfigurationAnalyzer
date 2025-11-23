//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   DriversDetailControl.xaml.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;


public sealed partial class DriversDetailControl : UserControl
{


    public static readonly DependencyProperty ListDetailsMenuItemProperty =
        DependencyProperty.Register("ListDetailsMenuItem", typeof(SampleOrder), typeof(DriversDetailControl),
            new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));





    public DriversDetailControl()
    {
        InitializeComponent();
    }





    public SampleOrder? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as SampleOrder;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }





    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DriversDetailControl control) control.ForegroundElement.ChangeView(0, 0, 1);
    }


}
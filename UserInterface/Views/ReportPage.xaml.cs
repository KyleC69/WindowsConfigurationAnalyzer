//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   ReportPage.xaml.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

using Microsoft.UI.Xaml.Controls;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;


public sealed partial class ReportPage : Page
{


    public ReportPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<ReportViewModel>();
    }





    public ReportViewModel ViewModel { get; }


}
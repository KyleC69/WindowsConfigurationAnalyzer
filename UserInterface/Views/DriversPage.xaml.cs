//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   DriversPage.xaml.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using CommunityToolkit.WinUI.UI.Controls;

using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

using Microsoft.UI.Xaml.Controls;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;


public sealed partial class DriversPage : Page
{


    public DriversPage()
    {
        ViewModel = App.GetService<DriversViewModel>();
        InitializeComponent();
    }





    public DriversViewModel ViewModel { get; }





    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {
        if (e == ListDetailsViewState.Both) ViewModel.EnsureItemSelected();
    }


}
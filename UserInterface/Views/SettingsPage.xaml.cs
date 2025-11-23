//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   SettingsPage.xaml.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Runtime.Versioning;

using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;


// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
[SupportedOSPlatform("windows10.0.22601.0")]
public sealed partial class SettingsPage : Page
{


    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }





    public SettingsViewModel ViewModel { get; }





    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // Ensure settings are loaded so bindings (e.g., IsActivityLoggingEnabled) reflect persisted values
        // LoadAsync is internal to the ViewModel and invoked in constructor, but ensure it's completed here
        // by re-invoking a lightweight read pattern if needed in the future.
        // Currently LoadAsync is started in constructor; nothing else required here.
        await Task.CompletedTask;
    }


}
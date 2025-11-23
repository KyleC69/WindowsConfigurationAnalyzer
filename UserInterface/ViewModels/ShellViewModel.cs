//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   ShellViewModel.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using CommunityToolkit.Mvvm.ComponentModel;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Views;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;


public partial class ShellViewModel : ObservableRecipient
{


    private bool _isBackEnabled;

    private object? _selected;





    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }





    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }


    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }


    public INavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; }





    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        _isBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            _selected = NavigationViewService.SettingsItem;

            return;
        }

        NavigationViewItem? selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null) _selected = selectedItem;
    }


}
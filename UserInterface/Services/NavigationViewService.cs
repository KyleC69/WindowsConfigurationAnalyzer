//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   NavigationViewService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Diagnostics.CodeAnalysis;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;
using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

using Microsoft.UI.Xaml.Controls;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Services;


public class NavigationViewService(INavigationService navigationService, IPageService pageService)
    : INavigationViewService
{


    private NavigationView? _navigationView;


    public IList<object>? MenuItems => _navigationView?.MenuItems;

    public object? SettingsItem => _navigationView?.SettingsItem;





    [MemberNotNull(nameof(_navigationView))]
    public void Initialize(NavigationView navigationView)
    {
        _navigationView = navigationView;
        _navigationView.BackRequested += OnBackRequested;
        _navigationView.ItemInvoked += OnItemInvoked;
    }





    public void UnregisterEvents()
    {
        if (_navigationView != null)
        {
            _navigationView.BackRequested -= OnBackRequested;
            _navigationView.ItemInvoked -= OnItemInvoked;
        }
    }





    public NavigationViewItem? GetSelectedItem(Type pageType)
    {
        return _navigationView != null
            ? GetSelectedItem(_navigationView.MenuItems, pageType) ??
              GetSelectedItem(_navigationView.FooterMenuItems, pageType)
            : null;
    }





    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        navigationService.GoBack();
    }





    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        }
        else
        {
            NavigationViewItem? selectedItem = args.InvokedItemContainer as NavigationViewItem;

            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey) navigationService.NavigateTo(pageKey);
        }
    }





    private NavigationViewItem? GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
    {
        foreach (NavigationViewItem item in menuItems.OfType<NavigationViewItem>())
        {
            if (IsMenuItemForPageType(item, pageType)) return item;

            NavigationViewItem? selectedChild = GetSelectedItem(item.MenuItems, pageType);

            if (selectedChild != null) return selectedChild;
        }

        return null;
    }





    private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
    {
        return menuItem.GetValue(NavigationHelper.NavigateToProperty) is string pageKey &&
               pageService.GetPageType(pageKey) == sourcePageType;
    }


}
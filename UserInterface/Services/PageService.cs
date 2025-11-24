//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   PageService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using CommunityToolkit.Mvvm.ComponentModel;

using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;
using KC.WindowsConfigurationAnalyzer.UserInterface.Views;

using Microsoft.UI.Xaml.Controls;

using ApplicationsPage = KC.WindowsConfigurationAnalyzer.UserInterface.Views.ApplicationsPage;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Services;


public class PageService : IPageService
{


    private readonly Dictionary<string, Type> _pages = [];





    public PageService()
    {
        Configure<WorkflowViewModel, WorkflowPage>();
        Configure<ServicesViewModel, ServicesPage>();
        Configure<DriversViewModel, DriversPage>();
        Configure<WmiRegistryViewModel, WmiRegistryPage>();
        Configure<ApplicationsViewModel, ApplicationsPage>();
        Configure<SettingsViewModel, SettingsPage>();
        Configure<AnalyzerViewModel, AnalyzerPage>();
        Configure<EventingViewModel, EventingPage>();
    }





    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType)) throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
        }

        return pageType;
    }





    private void Configure<TVm, TV>()
        where TVm : ObservableObject
        where TV : Page
    {
        lock (_pages)
        {
            string key = typeof(TVm).FullName!;

            if (_pages.ContainsKey(key)) throw new ArgumentException($"The key {key} is already configured in PageService");

            Type type = typeof(TV);

            if (_pages.ContainsValue(type))
                throw new ArgumentException(
                    $"This type is already configured with key {_pages.First(p => p.Value == type).Key}");

            _pages.Add(key, type);
        }
    }


}
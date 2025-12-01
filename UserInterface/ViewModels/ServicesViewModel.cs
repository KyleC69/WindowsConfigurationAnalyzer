//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   ServicesViewModel.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.ViewModels;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;


public partial class ServicesViewModel(ISampleDataService sampleDataService) : ObservableRecipient, INavigationAware
{


    private SampleOrder? _selected;


    public SampleOrder? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }


    public ObservableCollection<SampleOrder> SampleItems { get; } = [];





    public async void OnNavigatedTo(object parameter)
    {
        SampleItems.Clear();

        // TODO: Replace with real data.
        IEnumerable<SampleOrder> data = await sampleDataService.GetListDetailsDataAsync();

        foreach (SampleOrder item in data)
        {
            SampleItems.Add(item);
        }
    }





    public void OnNavigatedFrom()
    {
    }





    public void EnsureItemSelected()
    {
        Selected ??= SampleItems.First();
    }


}
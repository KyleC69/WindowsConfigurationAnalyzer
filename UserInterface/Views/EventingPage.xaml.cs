//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   EventingPage.xaml.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Views;


public sealed partial class EventingPage : Page
{


    public EventingPage()
    {
        ViewModel = App.GetService<EventingViewModel>();
        InitializeComponent();

        DataContext = ViewModel;

        Loaded += Page_Loaded;
    }





    public EventingViewModel ViewModel { get; }





    public void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.LogNames.Count == 0) ViewModel.OnNavigatedTo(null!);

        LogSelectorComboBox.SelectedIndex = 0;
    }


}
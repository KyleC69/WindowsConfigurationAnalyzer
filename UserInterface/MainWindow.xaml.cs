//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   MainWindow.xaml.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

using Microsoft.UI.Dispatching;

using Windows.UI.ViewManagement;

using WinUIEx;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface;


public sealed partial class MainWindow : WindowEx
{


    private readonly DispatcherQueue _dispatcherQueue;

    private readonly UISettings _settings;





    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _settings = new UISettings();
        _settings.ColorValuesChanged +=
            Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event
    }





    // this handles updating the caption button colors correctly when indows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        _dispatcherQueue.TryEnqueue(TitleBarHelper.ApplySystemThemeToCaptionButtons);
    }


}
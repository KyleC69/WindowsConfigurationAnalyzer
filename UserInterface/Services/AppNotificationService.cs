//  Created:  2025/11/10
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   AppNotificationService.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Collections.Specialized;
using System.Web;

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;

using Microsoft.Windows.AppNotifications;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Services;


public class AppNotificationService : IAppNotificationService
{


    private readonly INavigationService _navigationService;





    public AppNotificationService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }





    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;

        AppNotificationManager.Default.Register();
    }





    public bool Show(string payload)
    {
        AppNotification appNotification = new(payload);

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }





    public NameValueCollection ParseArguments(string arguments)
    {
        return HttpUtility.ParseQueryString(arguments);
    }





    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }





    ~AppNotificationService()
    {
        Unregister();
    }





    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        // TODO: Handle notification invocations when your app is already running.

        //// // Navigate to a specific page based on the notification arguments.
        //// if (ParseArguments(args.Argument)["action"] == "Settings")
        //// {
        ////    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        ////    {
        ////        _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        ////    });
        //// }

        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            App.MainWindow.ShowMessageDialogAsync(
                "TODO: Handle notification invocations when your app is already running.", "Notification Invoked");

            App.MainWindow.BringToFront();
        });
    }


}
//  Created:  2025/11/10
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   AppNotificationActivationHandler.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Activation;


public class AppNotificationActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{


    private readonly INavigationService _navigationService;
    private readonly IAppNotificationService _notificationService;





    public AppNotificationActivationHandler(INavigationService navigationService,
        IAppNotificationService notificationService)
    {
        _navigationService = navigationService;
        _notificationService = notificationService;
    }





    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        return AppInstance.GetCurrent().GetActivatedEventArgs()?.Kind == ExtendedActivationKind.AppNotification;
    }





    protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        // TODO: Handle notification activations.

        //// // Access the AppNotificationActivatedEventArgs.
        //// var activatedEventArgs = (AppNotificationActivatedEventArgs)AppInstance.GetCurrent().GetActivatedEventArgs().Data;

        //// // Navigate to a specific page based on the notification arguments.
        //// if (_notificationService.ParseArguments(activatedEventArgs.Argument)["action"] == "Settings")
        //// {
        ////     // Queue navigation with low priority to allow the UI to initialize.
        ////     App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
        ////     {
        ////         _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        ////     });
        //// }

        App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low,
            () =>
            {
                App.MainWindow.ShowMessageDialogAsync("TODO: Handle notification activations.",
                    "Notification Activation");
            });

        await Task.CompletedTask;
    }


}
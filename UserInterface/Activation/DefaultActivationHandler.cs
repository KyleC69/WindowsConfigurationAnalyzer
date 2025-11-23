//  Created:  2025/10/29
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   DefaultActivationHandler.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.UserInterface.ViewModels;

using Microsoft.UI.Xaml;

#endregion





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Activation;


public class DefaultActivationHandler(INavigationService navigationService)
    : ActivationHandler<LaunchActivatedEventArgs>
{


    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return navigationService.Frame?.Content == null;
    }





    protected override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        navigationService.NavigateTo(typeof(ReportViewModel).FullName!, args.Arguments);

        return Task.CompletedTask;
    }


}
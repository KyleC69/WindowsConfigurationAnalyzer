//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   ActivationHandler.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Activation;


// Extend this class to implement new ActivationHandlers. See DefaultActivationHandler for an example.
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md
public abstract class ActivationHandler<T> : IActivationHandler
    where T : class
{


    public bool CanHandle(object args)
    {
        return args is T && CanHandleInternal((args as T)!);
    }





    public async Task HandleAsync(object args)
    {
        await HandleInternalAsync((args as T)!);
    }





    // Override this method to add the logic for whether to handle the activation.
    protected virtual bool CanHandleInternal(T args)
    {
        return true;
    }





    // Override this method to add the logic for your activation handler.
    protected abstract Task HandleInternalAsync(T args);


}
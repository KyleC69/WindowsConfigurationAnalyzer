//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   ActivityLogAdapter.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using KC.WindowsConfigurationAnalyzer.Contracts;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


public class ActivityLogAdapter : IActivityLogger
{


    public void Log(string level, string message, string context)
    {
        ActivityLogger.Log(level, message, context);
    }





    public void Info(string context, string action, string message)
    {
        ActivityLogger.Log("INF", message, context + action);
    }





    public void Warning(string context, string action, string message)
    {
        ActivityLogger.Log("WRN", message, context + action);
    }





    public void Error(string context, string action, string message, Exception? ex)
    {
        ActivityLogger.Log("ERR", message, context + action);
    }


}
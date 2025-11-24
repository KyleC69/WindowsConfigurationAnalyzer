//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   IActivityLogger.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IActivityLogger
{


    void Log(string level, string message, string context);

    void Info(string context, string action, string message);

    void Warning(string context, string action, string message);

    void Error(string context, string action, string message, Exception? ex);


}
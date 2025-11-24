//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   RuleRunner.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts;

#endregion





// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer;


//Class will serve as the main entry point for running rules
public class RuleRunner
{


    private readonly IActivityLogger _logger;





    public RuleRunner(IActivityLogger logger)
    {
        _logger = logger;
    }


}
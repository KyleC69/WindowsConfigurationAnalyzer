//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   InitializeEngine.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;

#endregion





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


internal class InitializeEngine
{


    private readonly IActivityLogger _logger;
    private WorkflowOrchestrator _orchestrator = null!;





    public InitializeEngine(IActivityLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }





    public WorkflowOrchestrator GetOrchestrator()
    {
        List<IProbe> probes =
        [
            // Add probe implementations here
            // new RegistryReader(_logger),
            // new CimReader(_logger),
            // new FileSystemReader(_logger),
            new EnvironmentReader(_logger),
            new CimReader(_logger),
            new RegistryReader(_logger),
            new FileSystemReader(_logger),
            new AclReader(_logger)
        ];

        _orchestrator = new WorkflowOrchestrator(probes);


        return _orchestrator;
    }


}
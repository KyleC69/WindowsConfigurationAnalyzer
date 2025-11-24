//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   IAnalyzerModule.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Collections.ObjectModel;

#endregion





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IAnalyzerModule
{


    string Name { get; }

    string Area { get; }

    Task<AreaResult> AnalyzeAsync(IActivityLogger logger, IAnalyzerContext context, CancellationToken cancellationToken);


}



public class AreaResult
{


    public AreaResult(string area, object summary, object details, ICollection<Finding> asReadOnly, ICollection<string> warnings, List<string> errors)
    {
    }


}
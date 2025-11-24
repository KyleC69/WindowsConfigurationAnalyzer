//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   IExporter.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

#endregion





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public interface IExporter
{


    Task ExportAsync(AnalyzerResult result, string targetPath, CancellationToken cancellationToken);


}



public class AnalyzerResult
{


    public AnalyzerResult(string machineName, DateTimeOffset utcNow, List<AreaResult> areaResults, List<Finding> globalFindings, object areas)
    {
        Areas = areas;
    }





    public object Areas { get; set; }


}
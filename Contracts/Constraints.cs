//  Created:  2025/11/23
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   Constraints.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.

namespace KC.WindowsConfigurationAnalyzer.Contracts;


public class ExecutionConstraints
{


    public bool RunOncePerSession { get; set; }
    public TimeSpan Timeout { get; set; }
    public int RetryCount { get; set; }


}



public class ScoringModule
{


    public int SeverityScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // e.g., Low, Medium, High


}



public class AuditModule
{


    public bool EnableAuditTrail { get; set; }
    public string AuditLogTarget { get; set; } = string.Empty;


}



public class DependencyModule
{


    public List<string> ParentRules { get; set; } = [];
    public List<string> ChildRules { get; set; } = [];


}



public class TaggingModule
{


    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];


}



public class ResultSchemaModule
{


    public string Format { get; set; } = string.Empty; // JSON, XML, Table
    public bool IncludeTimestamp { get; set; } = true;


}
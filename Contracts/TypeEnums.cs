//  Created:  2025/11/17
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   TypeEnums.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Text.Json.Serialization;

#endregion





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public class RuleParameter
{


    public string Name { get; set; } = string.Empty;
    public ParameterType Type { get; set; }
    public object? DefaultValue { get; set; }


}



public enum ParameterType
{


    String,
    Int,
    Bool,
    Date,
    Array


}



public class ProbeTarget
{


    public SubsystemType Subsystem { get; set; }
    public string Location { get; set; } = string.Empty; // registry path, file path, ETW provider, etc.
    public string Pattern { get; set; } = string.Empty; // optional regex or wildcard


}



public enum SubsystemType
{


    Registry,
    FileSystem,
    ETW,
    ACL,
    Profile


}



public class ExpressionDefinition
{


    public ExpressionType ExpressionType { get; set; }
    public string ExpressionText { get; set; } = string.Empty;


}



[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExpressionType
{


    Comparison,
    Logical,
    Regex,
    Math,
    FunctionCall,
    LambdaExpression,
    Predicate,
    RegexMatch,
    Contains,
    Script,
    ETWProbe,
    AclEvaluation,
    HashVerification


}



public class RuleAction
{


    public ActionType ActionType { get; set; }
    public string Target { get; set; } = string.Empty; // log file, notification channel, etc.


}



public enum ActionType
{


    Log,
    Notify,
    Mark,
    Task


}



public class ExecutionConstraints
{


    public bool RunOncePerSession { get; set; } = false;
    public bool RequireParentSuccess { get; set; } = false;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int RetryCount { get; set; } = 0;


}
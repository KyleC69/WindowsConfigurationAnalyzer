//  Created:  2025/11/24
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   ExpressionType.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using System.Text.Json.Serialization;





namespace KC.WindowsConfigurationAnalyzer.Contracts;


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
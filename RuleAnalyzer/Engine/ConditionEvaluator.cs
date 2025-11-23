//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   ConditionEvaluator.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Text.RegularExpressions;

#endregion





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


public static class ConditionEvaluator
{


    public static bool Evaluate(object? actual, string op, object? expected)
    {
        return op switch
        {
            "Equals" => Equals(actual, expected),
            "NotEquals" => !Equals(actual, expected),
            "GreaterThan" => Compare(actual, expected) > 0,
            "LessThan" => Compare(actual, expected) < 0,
            "Contains" => actual?.ToString()?.Contains(expected?.ToString() ?? "") ?? false,
            "NotContains" => !(actual?.ToString()?.Contains(expected?.ToString() ?? "") ?? false),
            "RegexMatch" => Regex.IsMatch(actual?.ToString() ?? "", expected?.ToString() ?? ""),
            "Exists" => actual != null,
            "NotExists" => actual == null,
            _ => false,
        };
    }





    private static int Compare(object? a, object? b)
    {
        return a is IComparable ca && b is IComparable cb ? ca.CompareTo(cb) : 0;
    }


}
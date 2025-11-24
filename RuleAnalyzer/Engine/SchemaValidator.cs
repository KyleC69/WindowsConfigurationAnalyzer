//  Created:  2025/11/23
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   SchemaValidator.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Text.Json;

using KC.WindowsConfigurationAnalyzer.Contracts;

using NJsonSchema;
using NJsonSchema.Validation;

#endregion





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


public class SchemaValidator
{


    private readonly IActivityLogger _logger;
    private readonly JsonSchema _ruleSchema;
    private readonly JsonSchema _workflowSchema;





    public SchemaValidator(string workflowSchemaPath, string ruleSchemaPath, IActivityLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _workflowSchema = JsonSchema.FromUrlAsync(workflowSchemaPath).Result;
        _ruleSchema = JsonSchema.FromUrlAsync(ruleSchemaPath).Result;

        //    _logger!.Log("ERR", "Unable to reach Schema server, check internet options and try again.", "SchemaURLFailure");
        //  WCAEventSource.Log.NetworkError("","https",workflowSchemaPath,443,$"Unable to reach Schema server ex.ToString()","");
    }





    internal bool ValidateWorkflow(JsonDocument workflowJson, out ICollection<ValidationError> results)
    {
        results = _workflowSchema.Validate(workflowJson.ToString()!);

        foreach (JsonElement rule in workflowJson.RootElement.GetProperty("Rules").EnumerateArray())
        {
            ICollection<ValidationError> ruleResults = ValidateRule(rule, out _) ? Array.Empty<ValidationError>() : _ruleSchema.Validate(rule.ToString()!);
            foreach (ValidationError error in ruleResults)
            {
                results.Add(error);
            }
        }

        if (results.Count > 0)
        {
            _logger.Log("ERR", "Workflow schema validation failed with the following errors:", "SchemaValidator");
            foreach (ValidationError error in results)
            {
                _logger.Log("ERR", $"- {error.Path}: {error.Kind}", "SchemaValidator");
            }
        }

        return results.Count == 0;
    }





    public bool ValidateRule(JsonElement ruleJson, out ICollection<ValidationError> results)
    {
        results = _ruleSchema.Validate(ruleJson.ToString()!);

        return results.Count == 0;
    }


}
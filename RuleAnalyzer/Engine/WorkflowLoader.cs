//  Created:  2025/11/23
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   WorkflowLoader.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Text.Json;

using NJsonSchema.Validation;

#endregion





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Engine;


public class WorkflowLoader
{


    private readonly ApplicabilityEvaluator _evaluator;
    private readonly SchemaValidator _validator;





    public WorkflowLoader(SchemaValidator validator, ApplicabilityEvaluator evaluator)
    {
        _validator = validator;
        _evaluator = evaluator;
    }





    public void LoadWorkflow(string workflowPath)
    {
        JsonDocument workflowJson = JsonDocument.Parse(File.ReadAllText(workflowPath));

        if (!_validator.ValidateWorkflow(workflowJson, out ICollection<ValidationError> results))
        {
            throw new InvalidOperationException($"Workflow schema validation failed: {results}");
        }

        JsonElement applicability = workflowJson.RootElement.GetProperty("Applicability");

        if (!_evaluator.IsApplicable(applicability))
        {
            throw new InvalidOperationException("Workflow not applicable to this machine.");
        }

        Console.WriteLine("Workflow loaded successfully and is applicable.");
    }


}
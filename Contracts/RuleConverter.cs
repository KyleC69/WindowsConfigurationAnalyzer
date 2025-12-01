//  Created:  2025/11/24
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   RuleConverter.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




using Newtonsoft.Json;
using Newtonsoft.Json.Linq;





namespace KC.WindowsConfigurationAnalyzer.Contracts;


public class RuleConverter : JsonConverter<RuleContract>
{


    public override RuleContract ReadJson(JsonReader reader, Type objectType, RuleContract? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        JToken? providerToken = jo["Provider"];
        ProviderType provider = default;
        if (providerToken != null && providerToken.Type != JTokenType.Null) Enum.TryParse(providerToken.ToString() ?? string.Empty, out provider);

        var rule = new RuleContract
        {
            RuleName = jo["RuleName"]?.ToString()!,
            Provider = provider,
            Condition = jo["Condition"]?.ToObject<Condition>(serializer)!,
            Severity = jo["Severity"]?.ToObject<int>() ?? 0,
            Message = jo["Message"]?.ToString()!,
            Tags = jo["Tags"]?.ToObject<List<string>>(serializer)!,
            Execution = jo["Execution"]?.ToObject<ExecutionOptions>(serializer)!,
            Parameters = null
        };

        JToken? parametersToken = jo["Parameters"];
        rule.Parameters = rule.Provider switch
        {
            ProviderType.Registry => parametersToken?.ToObject<RegistryParameters>(serializer)!,
            ProviderType.FileSystem => parametersToken?.ToObject<FileSystemParameters>(serializer)!,
            ProviderType.ACL => parametersToken?.ToObject<AclParameters>(serializer)!,
            ProviderType.WMI => parametersToken?.ToObject<WmiParameters>(serializer)!,
            ProviderType.EventLog => parametersToken?.ToObject<EventLogParameters>(serializer)!,
            ProviderType.Service => parametersToken?.ToObject<ServiceParameters>(serializer)!,
            ProviderType.Process => parametersToken?.ToObject<ProcessParameters>(serializer)!,
            ProviderType.Custom => parametersToken?.ToObject<CustomParameters>(serializer)!,
            _ => throw new JsonSerializationException($"Unknown provider: {rule.Provider}")
        };

        return rule;
    }





    public override void WriteJson(JsonWriter writer, RuleContract value, JsonSerializer serializer)
    {
        JObject jo = JObject.FromObject(value, serializer);
        jo.WriteTo(writer);
    }


}
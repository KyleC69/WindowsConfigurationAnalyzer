//  Created:  2025/11/23
// Solution:  WindowsConfigurationAnalyzer
//   Project:  Contracts
//        File:   AclEntry.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.Contracts;


// Canonical ACE representation for files and registry keys
public record AclEntry
{


    public string IdentityReference { get; init; } = string.Empty;
    public string Rights { get; init; } = string.Empty; // "Read;Write;FullControl"
    public string InheritanceFlags { get; init; } = string.Empty;
    public bool IsInherited { get; init; }
    public string RawSddl { get; init; } = string.Empty;


}





// Registry key/value snapshot



// Parsed/normalized manifest structure (light)



// Top-level facts object passed to rules engine



// Audit-grade rule result artifact (wrapper for RulesEngine output + evidence)
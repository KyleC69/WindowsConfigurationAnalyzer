//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface.Core
//        File:   SampleCompany.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;


// Model for the SampleDataService. Replace with your own model.
public class SampleCompany
{


    public string? CompanyID { get; set; }

    public string? CompanyName { get; set; }

    public string? ContactName { get; set; }

    public string? ContactTitle { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public ICollection<SampleOrder>? Orders { get; set; }


}
//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface.Core
//        File:   SampleOrder.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;


// Model for the SampleDataService. Replace with your own model.
public class SampleOrder
{


    public long OrderID { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime RequiredDate { get; set; }

    public DateTime ShippedDate { get; set; }

    public string? ShipperName { get; set; }

    public string? ShipperPhone { get; set; }

    public double Freight { get; set; }

    public string? Company { get; set; }

    public string? ShipTo { get; set; }

    public double OrderTotal { get; set; }

    public string? Status { get; set; }

    public int SymbolCode { get; set; }

    public string? SymbolName { get; set; }

    public char Symbol => (char)SymbolCode;

    public ICollection<SampleOrderDetail>? Details { get; set; }

    public string ShortDescription => $"Order ID: {OrderID}";





    public override string ToString()
    {
        return $"{Company} {Status}";
    }


}
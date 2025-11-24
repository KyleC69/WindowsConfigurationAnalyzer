//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface.Core
//        File:   SampleOrderDetail.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;


// Model for the SampleDataService. Replace with your own model.
public class SampleOrderDetail
{


    public long ProductID { get; set; }

    public string? ProductName { get; set; }

    public int Quantity { get; set; }

    public double Discount { get; set; }

    public string? QuantityPerUnit { get; set; }

    public double UnitPrice { get; set; }

    public string? CategoryName { get; set; }

    public string? CategoryDescription { get; set; }

    public double Total { get; set; }

    public string ShortDescription => $"Product ID: {ProductID} - {ProductName}";


}
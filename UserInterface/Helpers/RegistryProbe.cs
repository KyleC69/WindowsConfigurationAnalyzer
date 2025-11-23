//  Created:  2025/11/17
// Solution:  WindowsConfigurationAnalyzer
//   Project:  UserInterface
//        File:   RegistryProbe.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


public class RegistryProbe
{


    private readonly string _registryKeyPath;
    private readonly string _registryValueName;





    public RegistryProbe(string registryKeyPath, string registryValueName)
    {
        _registryKeyPath = registryKeyPath;
        _registryValueName = registryValueName;
    }





    public string ReadRegistryValue()
    {
        // In a real application, you would use Microsoft.Win32.Registry to read the value.
        // For demonstration purposes, we'll return a placeholder.
        // Example:
        // using Microsoft.Win32;
        // try
        // {
        //     using (RegistryKey key = Registry.LocalMachine.OpenSubKey(_registryKeyPath))
        //     {
        //         if (key != null)
        //         {
        //             return key.GetValue(_registryValueName)?.ToString();
        //         }
        //     }
        // }
        // catch (System.Exception)
        // {
        //     // Handle exceptions appropriately
        // }

        return $"Placeholder value for Key: {_registryKeyPath}, Value: {_registryValueName}";
    }


}
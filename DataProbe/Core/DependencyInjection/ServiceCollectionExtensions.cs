//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   ServiceCollectionExtensions.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using KC.WindowsConfigurationAnalyzer.Contracts;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Drivers;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.EventLog;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Hardware;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Network;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.OS;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Performance;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Policy;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Security;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Software;
using KC.WindowsConfigurationAnalyzer.DataProbe.Areas.Startup;

using Microsoft.Extensions.DependencyInjection;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.DependencyInjection;


public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddWcaCore(this IServiceCollection services)
    {
        // Readers



        // Default module registrations (host can add/override as needed)
        services.AddSingleton<IAnalyzerModule, OSAnalyzer>();
        services.AddSingleton<IAnalyzerModule, HardwareAnalyzer>();
        services.AddSingleton<IAnalyzerModule, NetworkAnalyzer>();
        services.AddSingleton<IAnalyzerModule, SecurityAnalyzer>();
        services.AddSingleton<IAnalyzerModule, SoftwareAnalyzer>();
        services.AddSingleton<IAnalyzerModule, PerformanceAnalyzer>();
        services.AddSingleton<IAnalyzerModule, PolicyAnalyzer>();
        services.AddSingleton<IAnalyzerModule, StartupAnalyzer>();
        services.AddSingleton<IAnalyzerModule, EventLogAnalyzer>();
        services.AddSingleton<IAnalyzerModule, DriversAnalyzer>();

        return services;
    }


}
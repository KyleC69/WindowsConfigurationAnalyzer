//  Created:  2025/11/22
// Solution:  WindowsConfigurationAnalyzer
//   Project:  DataProbe
//        File:   AclReader.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Security.AccessControl;
using System.Security.Principal;

using KC.WindowsConfigurationAnalyzer.Contracts;

#endregion





namespace KC.WindowsConfigurationAnalyzer.DataProbe.Core.Readers;


public class AclReader : IProbe
{


    private readonly IActivityLogger _logger;





    public AclReader(IActivityLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }





    public string Provider => "Acl";





    public async Task<ProbeResult> ExecuteAsync(IDictionary<string, object> parameters, CancellationToken token, string callerName = "", string callerFilePath = "")
    {
        ProbeResult result = new()
        {
            Provider = Provider,
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>()
        };
        string? path = parameters["path"].ToString();

        try
        {
            // Implement ACL-specific probing logic here
            object? aclInfo = GetAclInformation(path);
            result.Value = aclInfo;
            result.Metadata["path"] = path;

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.Log("ERR", $"Error executing ACL on path: {path} probe: {ex.Message}, {callerName}, {callerFilePath}", "AclReader");
            result.ProbeSuccess = false;
            result.Message = "Error executing ACL probe.";
            result.Value = null;
            result.Metadata["Error"] = ex.Message;

            return await Task.FromResult(result);
        }
    }





    private object? GetAclInformation(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            _logger.Log("WARN", "Path is null or empty.", "AclReader");

            return null;
        }

        //Is path a directory or a file?
        try
        {
            bool isDirectory = Directory.Exists(path);
            FileSystemSecurity securityInfo = isDirectory
                ? new DirectoryInfo(path).GetAccessControl()
                : new FileInfo(path).GetAccessControl();


            AuthorizationRuleCollection rules = securityInfo.GetAccessRules(true, true, typeof(NTAccount));
            List<string> aclInfo = [];

            foreach (FileSystemAccessRule rule in rules)
            {
                aclInfo.Add($"{rule.IdentityReference.Value}: {rule.AccessControlType} - {rule.FileSystemRights}");
            }

            return aclInfo;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Error("AclReader", "GetAclInformation", $"Access denied to path: {path}", ex);

            return null;
        }
        catch (Exception ex)
        {
            _logger.Error("AclReader", "GetAclInformation", $"Unexpected error for path: {path}", ex);

            return null;
        }
    }


}
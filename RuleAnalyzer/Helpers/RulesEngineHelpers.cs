//  Created:  2025/11/16
// Solution:  WindowsConfigurationAnalyzer
//   Project:  RuleAnalyzer
//        File:   RulesEngineHelpers.cs
//  Author:    Kyle Crowder
// 
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.




#region

using System.Security.Cryptography;
using System.Text.RegularExpressions;

using KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Models;

#endregion





namespace KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Helpers;


public static class RulesEngineHelpers
{


    // File existence check (returns boolean)
    public static bool FileExists(string? path)
    {
        return string.IsNullOrWhiteSpace(path) ? false : File.Exists(path);
    }





    // Compute SHA256 of a file and return lowercase hex; returns null on failure
    public static string? ComputeFileSha256(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        using FileStream fs = File.OpenRead(path);
        using SHA256 sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(fs);

        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }





    // Compare manifest channel names to registry channel names with normalization
    public static bool AreNamesEquivalent(IEnumerable<string>? manifestNames, IEnumerable<string>? registryNames)
    {
        if (manifestNames == null || registryNames == null)
        {
            return false;
        }

        static string normalize(string s)
        {
            if (s == null)
            {
                return string.Empty;
            }

            // canonicalize separators, trim, collapse repeated separators, lowercase
            string t = s.Trim().Replace('\\', '/').Replace('-', '/');
            t = Regex.Replace(t, "/{2,}", "/");

            return t.Trim('/').ToLowerInvariant();
        }

        List<string> m = manifestNames.Select(normalize).Where(x => x.Length > 0).OrderBy(x => x).ToList();
        List<string> r = registryNames.Select(normalize).Where(x => x.Length > 0).OrderBy(x => x).ToList();

        return m.SequenceEqual(r);
    }





    // Evaluate file ACLs against a policy. Returns true if ACLs are acceptable.
    // Policy (example): deny Everyone/Auth Users write/modify/fullcontrol; allow SYSTEM/Admins read+execute; allow specific service accounts write.
    public static bool EvaluateFileAcl(IEnumerable<AclEntry>? aclEntries, out Dictionary<string, object> evidence)
    {
        evidence = [];
        if (aclEntries == null)
        {
            evidence["reason"] = "no-acl";

            return false;
        }

        List<AclEntry> list = aclEntries.ToList();
        evidence["entryCount"] = list.Count;

        // identities that should not have write rights
        string[] forbiddenIdentities = new[] { "EVERYONE", "NT AUTHORITY\\AUTHENTICATED USERS", "AUTHENTICATED USERS" };
        // rights that imply write/control
        string[] writeIndicators = new[] { "write", "modify", "fullcontrol", "delete", "changepermissions", "takeownership" };

        List<object> violations = [];
        foreach (AclEntry ace in list)
        {
            string id = ace.IdentityReference?.ToUpperInvariant().Trim() ?? string.Empty;
            string rights = ace.Rights?.ToLowerInvariant() ?? string.Empty;

            bool hasWrite = writeIndicators.Any(rights.Contains);
            if (hasWrite && forbiddenIdentities.Any(id.Contains))
            {
                violations.Add(new { identity = ace.IdentityReference, rights = ace.Rights, raw = ace.RawSddl });
            }
        }

        evidence["violations"] = violations;

        return violations.Count == 0;
    }





    // Evaluate registry ACLs similarly. Returns true if ACLs are acceptable.
    public static bool EvaluateRegistryAcl(IEnumerable<AclEntry>? aclEntries, out Dictionary<string, object> evidence)
    {
        evidence = [];
        if (aclEntries == null)
        {
            evidence["reason"] = "no-registry-acl";

            return false;
        }

        List<AclEntry> list = aclEntries.ToList();
        evidence["entryCount"] = list.Count;

        string[] forbiddenIdentities = new[] { "EVERYONE", "NT AUTHORITY\\AUTHENTICATED USERS", "AUTHENTICATED USERS" };
        string[] writeOps = new[] { "setvalue", "create subkey", "create sub key", "write", "fullcontrol", "delete" };

        List<object> violations = [];
        foreach (AclEntry ace in list)
        {
            string id = ace.IdentityReference?.ToUpperInvariant().Trim() ?? string.Empty;
            string rights = ace.Rights?.ToLowerInvariant() ?? string.Empty;

            bool grantsWrite = writeOps.Any(rights.Contains);
            if (grantsWrite && forbiddenIdentities.Any(id.Contains))
            {
                violations.Add(new { identity = ace.IdentityReference, rights = ace.Rights, raw = ace.RawSddl });
            }
        }

        evidence["violations"] = violations;

        return violations.Count == 0;
    }





    // Verify manifest hash against known registry-recorded value (registryValues may contain a manifest hash or resource reference)
    public static bool VerifyManifestHash(string? fileSha256, IEnumerable<RegistryKeySnapshot>? registrySnapshots, out Dictionary<string, object> evidence)
    {
        evidence = [];
        if (string.IsNullOrWhiteSpace(fileSha256))
        {
            evidence["reason"] = "no-file-hash";

            return false;
        }

        if (registrySnapshots == null)
        {
            evidence["reason"] = "no-registry-snapshots";

            return false;
        }

        // look for common registry values that might store a manifest reference or hash
        foreach (RegistryKeySnapshot snapshot in registrySnapshots)
        {
            foreach (RegistryValueEntry val in snapshot.Values)
            {
                string n = val.Name?.ToLowerInvariant() ?? string.Empty;
                string v = val.Value?.ToLowerInvariant() ?? string.Empty;
                if (n.Contains("hash") || n.Contains("manifest") || n.Contains("resource"))
                {
                    evidence[$"matchedKey:{snapshot.KeyPath}\\{val.Name}"] = val.Value ?? string.Empty;
                    // if val stores a hex hash, try to normalize and compare
                    string hex = Regex.Match(v ?? string.Empty, @"[0-9a-f]{32,}").Value;
                    if (!string.IsNullOrEmpty(hex) && hex.Length >= 32)
                    {
                        // normalize to lower-case full SHA256 length (64 hex)
                        if (hex.Length == 64 && hex == fileSha256.ToLowerInvariant())
                        {
                            evidence["match"] = true;

                            return true;
                        }
                    }
                }
            }
        }

        evidence["match"] = false;

        return false;
    }





    // Utility: normalize ACL entries from raw sddl or ACE strings (lightweight helper to parse "DOMAIN\\User;FullControl;IsInherited:false")
    // This does not replace System.Security APIs; it's a helper to normalize pre-parsed ACL lists.
    public static List<AclEntry> NormalizeAclEntries(IEnumerable<string>? rawAces)
    {
        List<AclEntry> list = [];

        if (rawAces == null)
        {
            return list;
        }

        foreach (string line in rawAces)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // expected format: "Identity;Rights;InheritanceFlags;IsInherited;RawSddl"
            string[] parts = line.Split(';');
            AclEntry e = new()
            {
                IdentityReference = parts.ElementAtOrDefault(0) ?? string.Empty,
                Rights = parts.ElementAtOrDefault(1) ?? string.Empty,
                InheritanceFlags = parts.ElementAtOrDefault(2) ?? string.Empty,
                IsInherited = bool.TryParse(parts.ElementAtOrDefault(3), out bool b) && b,
                RawSddl = parts.ElementAtOrDefault(4) ?? string.Empty
            };
            list.Add(e);
        }

        return list;
    }


}
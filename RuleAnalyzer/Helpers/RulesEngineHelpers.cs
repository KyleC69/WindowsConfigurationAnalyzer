using System.Security.Cryptography;
using System.Text.RegularExpressions;

using KC.WindowsConfigurationAnalyzer.RuleAnalyzer.Models;

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

        using var fs = File.OpenRead(path);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(fs);
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
            var t = s.Trim().Replace('\\', '/').Replace('-', '/');
            t = Regex.Replace(t, "/{2,}", "/");
            return t.Trim('/').ToLowerInvariant();
        }

        var m = manifestNames.Select(normalize).Where(x => x.Length > 0).OrderBy(x => x).ToList();
        var r = registryNames.Select(normalize).Where(x => x.Length > 0).OrderBy(x => x).ToList();
        return m.SequenceEqual(r);
    }

    // Evaluate file ACLs against a policy. Returns true if ACLs are acceptable.
    // Policy (example): deny Everyone/Auth Users write/modify/fullcontrol; allow SYSTEM/Admins read+execute; allow specific service accounts write.
    public static bool EvaluateFileAcl(IEnumerable<AclEntry>? aclEntries, out Dictionary<string, object> evidence)
    {
        evidence = new Dictionary<string, object>();
        if (aclEntries == null)
        {
            evidence["reason"] = "no-acl";
            return false;
        }

        var list = aclEntries.ToList();
        evidence["entryCount"] = list.Count;

        // identities that should not have write rights
        var forbiddenIdentities = new[] { "EVERYONE", "NT AUTHORITY\\AUTHENTICATED USERS", "AUTHENTICATED USERS" };
        // rights that imply write/control
        var writeIndicators = new[] { "write", "modify", "fullcontrol", "delete", "changepermissions", "takeownership" };

        var violations = new List<object>();
        foreach (var ace in list)
        {
            var id = ace.IdentityReference?.ToUpperInvariant().Trim() ?? string.Empty;
            var rights = ace.Rights?.ToLowerInvariant() ?? string.Empty;

            var hasWrite = writeIndicators.Any(rights.Contains);
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
        evidence = new Dictionary<string, object>();
        if (aclEntries == null)
        {
            evidence["reason"] = "no-registry-acl";
            return false;
        }

        var list = aclEntries.ToList();
        evidence["entryCount"] = list.Count;

        var forbiddenIdentities = new[] { "EVERYONE", "NT AUTHORITY\\AUTHENTICATED USERS", "AUTHENTICATED USERS" };
        var writeOps = new[] { "setvalue", "create subkey", "create sub key", "write", "fullcontrol", "delete" };

        var violations = new List<object>();
        foreach (var ace in list)
        {
            var id = ace.IdentityReference?.ToUpperInvariant().Trim() ?? string.Empty;
            var rights = ace.Rights?.ToLowerInvariant() ?? string.Empty;

            var grantsWrite = writeOps.Any(rights.Contains);
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
        evidence = new Dictionary<string, object>();
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
        foreach (var snapshot in registrySnapshots)
        {
            foreach (var val in snapshot.Values)
            {
                var n = val.Name?.ToLowerInvariant() ?? string.Empty;
                var v = val.Value?.ToLowerInvariant() ?? string.Empty;
                if (n.Contains("hash") || n.Contains("manifest") || n.Contains("resource"))
                {
                    evidence[$"matchedKey:{snapshot.KeyPath}\\{val.Name}"] = val.Value ?? string.Empty;
                    // if val stores a hex hash, try to normalize and compare
                    var hex = Regex.Match(v ?? string.Empty, @"[0-9a-f]{32,}").Value;
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
        var list = new List<AclEntry>();
        if (rawAces == null)
        {
            return list;
        }

        foreach (var line in rawAces)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            // expected format: "Identity;Rights;InheritanceFlags;IsInherited;RawSddl"
            var parts = line.Split(';');
            var e = new AclEntry
            {
                IdentityReference = parts.ElementAtOrDefault(0) ?? string.Empty,
                Rights = parts.ElementAtOrDefault(1) ?? string.Empty,
                InheritanceFlags = parts.ElementAtOrDefault(2) ?? string.Empty,
                IsInherited = bool.TryParse(parts.ElementAtOrDefault(3), out var b) && b,
                RawSddl = parts.ElementAtOrDefault(4) ?? string.Empty
            };
            list.Add(e);
        }
        return list;
    }
}

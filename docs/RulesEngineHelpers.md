Version:1.0StartHTML:00000097EndHTML:00012788StartFragment:00000153EndFragment:00012755

**Building workflow JSON**

I'm thinking about creating a workflow JSON with rules to check various components. I'll check if the provider is registered in HKLM\SOFTWARE, ensure the manifest file is there, and verify channel names match. The ACLs for registry keys and manifest resources should include specific SIDs (with placeholders to avoid being too exact). I'll consider including EventRegister and RulesEngine, and possibly add sources at the end of paragraphs with appropriate citations. I think I'm on the right track for this!

### Ruleset Overview

This RulesEngine workflow validates an EventSource provider’s manifest registration and its configuration surface, including manifest file presence, registry registration, channel-name parity, and ACLs on both the manifest file and the eventing registry channels/keys. It is written for Microsoft RulesEngine (JSON workflow format) and is intended to be executed from .NET with the RulesEngine library. It assumes the probe code supplies inputs (file system checks, registry reads, ACL reads, and command outputs) as named inputs described below.

> Inputs expected by the workflow (provided by your probe layer):
> 
> * **ProviderName**: string (EventSource provider name)
> * **ManifestPath**: string (full path to deployed manifest file)
> * **ManifestContent**: object (parsed manifest XML as object or null)
> * **RegistryKeys**: object (map of registry paths → properties/values read)
> * **RegisteredProviders**: array (list of provider names reported by EventRegister/wevtutil)
> * **ChannelNamesFromManifest**: array (channel names declared in manifest)
> * **ChannelNamesFromRegistry**: array (channel names read from registry bindings)
> * **FileAcl**: array (ACL entries on the manifest file; each entry: {IdentityReference, Rights, InheritanceFlags, IsInherited})
> * **RegistryAcls**: array (ACL entries on registry keys of interest; same entry shape as FileAcl)
> * **WevtutilQuery**: string (raw output when querying installed manifests or providers)
> * **ProbeTimestamp**: string (ISO timestamp for audit)

Use this ruleset as the canonical, provable check; attach probe evidence (raw outputs) to the rule results when executing.

* * *

### Top-level workflow JSON (RulesEngine)

    {
      "WorkflowName": "EventSourceProviderManifestRegistration",
      "Rules": [
        {
          "RuleName": "ProviderNameProvided",
          "Expression": "string.IsNullOrWhiteSpace(ProviderName) == false",
          "SuccessEvent": "Provider name provided",
          "ErrorMessage": "Provider name is missing"
        },
        {
          "RuleName": "ManifestFileExists",
          "Expression": "FileExists(ManifestPath) == true",
          "SuccessEvent": "Manifest file exists",
          "ErrorMessage": "Manifest file not found at ManifestPath"
        },
        {
          "RuleName": "ManifestParsed",
          "Expression": "ManifestContent != null",
          "SuccessEvent": "Manifest parsed",
          "ErrorMessage": "Manifest could not be parsed or returned empty"
        },
        {
          "RuleName": "ProviderRegisteredInRegistry",
          "Expression": "RegisteredProviders != null && RegisteredProviders.Contains(ProviderName)",
          "SuccessEvent": "Provider present in installed providers list",
          "ErrorMessage": "Provider not found among registered providers"
        },
        {
          "RuleName": "ProviderRegisteredInWevtutil",
          "Expression": "WevtutilQuery != null && WevtutilQuery.IndexOf(ProviderName, StringComparison.OrdinalIgnoreCase) >= 0",
          "SuccessEvent": "Provider listed in wevtutil output",
          "ErrorMessage": "Provider not listed by wevtutil query"
        },
        {
          "RuleName": "ManifestChannelsMatchRegistryBindings",
          "Expression": "ChannelNamesFromManifest != null && ChannelNamesFromRegistry != null && Enumerable.SequenceEqual(ChannelNamesFromManifest.OrderBy(c => c), ChannelNamesFromRegistry.OrderBy(c => c))",
          "SuccessEvent": "Manifest channel names match registry bindings",
          "ErrorMessage": "Channel names in manifest do not match registry bindings"
        },
        {
          "RuleName": "ManifestChannelNameNormalization",
          "Expression": "ChannelNamesFromManifest != null && ChannelNamesFromRegistry != null && AreNamesEquivalent(ChannelNamesFromManifest, ChannelNamesFromRegistry)",
          "SuccessEvent": "Channel names equivalent after normalization (slashes, dashes, casing)",
          "ErrorMessage": "Channel names mismatch even after normalization"
        },
        {
          "RuleName": "ManifestFileAclSecure",
          "Expression": "FileAcl != null && EvaluateFileAcl(FileAcl)",
          "SuccessEvent": "Manifest file ACLs meet policy",
          "ErrorMessage": "Manifest file ACLs violate expected policy"
        },
        {
          "RuleName": "RegistryAclsSecure",
          "Expression": "RegistryAcls != null && EvaluateRegistryAcl(RegistryAcls)",
          "SuccessEvent": "Registry ACLs meet policy",
          "ErrorMessage": "Registry ACLs violate expected policy"
        },
        {
          "RuleName": "ManifestInstallMatchesProviderHash",
          "Expression": "ManifestContent != null && RegistryKeys != null && VerifyManifestHash(ManifestContent, RegistryKeys)",
          "SuccessEvent": "Installed manifest hash matches registry-recorded hash",
          "ErrorMessage": "Mismatch between manifest file hash and registry manifest reference"
        },
        {
          "RuleName": "AuditEvidenceAttached",
          "Expression": "ProbeTimestamp != null && ProbeTimestamp != \"\"",
          "SuccessEvent": "Audit timestamp provided",
          "ErrorMessage": "Probe timestamp missing"
        }
      ],
      "WorkflowParameters": {
        "ProviderName": "",
        "ManifestPath": "",
        "ManifestContent": null,
        "RegistryKeys": null,
        "RegisteredProviders": null,
        "ChannelNamesFromManifest": null,
        "ChannelNamesFromRegistry": null,
        "FileAcl": null,
        "RegistryAcls": null,
        "WevtutilQuery": "",
        "ProbeTimestamp": ""
      }
    }

* * *

### Helper expression semantics and recommended probe-side functions

Implement the following helper functions in the execution host (C# probe layer or RulesEngine custom expression registration). The rules reference these symbols; RulesEngine supports registering custom functions to be used inside rule expressions.

* FileExists(string path)
  
  * returns bool: existence check for the manifest path.

* AreNamesEquivalent(string[] manifestNames, string[] registryNames)
  
  * normalization: replace forward/back slashes and dashes consistently, trim, collapse duplicate separators, and compare case-insensitively.
  * Purpose: detect known manifest vs. registry channel name divergences (e.g., slash vs. dash) as canonicalized equality.

* EvaluateFileAcl(array fileAclEntries)
  
  * Policy checks:
    * No Everyone or Authenticated Users with Write/Modify/Delete/FullControl.
    * Allowed write identities must be explicit service accounts (e.g., LocalService, NetworkService), Administrators only when justified.
    * Ensure inheritance flags and IsInherited are examined: prefer explicit ACLs over broad inherited writable ACLs.
  * Returns bool: true if the ACL set conforms to policy; false otherwise.

* EvaluateRegistryAcl(array registryAclEntries)
  
  * Policy checks:
    * Keys under HKLM\System\CurrentControlSet\Services\EventLog* have limited write access (Administrators and SYSTEM; service accounts only where required).
    * Provider-specific registration keys must not grant broad write to unprivileged groups.
    * Check for presence of explicit entries granting SetValue/CreateSubKey/DeleteKey; deny when applied to Everyone.
  * Returns bool.

* VerifyManifestHash(object manifestContent, object registryKeys)
  
  * If the registry stores the manifest resource reference or hash, verify they match the on-disk manifest content SHA256.
  * Returns bool.

Notes about implementing ACL checks:

* Represent ACL entries consistently: { "IdentityReference": "BUILTIN\Administrators", "Rights":"FullControl;ReadAndExecute", "InheritanceFlags":"ContainerInherit;ObjectInherit", "IsInherited":false }.
* Probe should include raw ACL strings and the normalized objects used for evaluation to preserve provenance and auditability.

(Implementations of these helpers are probe responsibilities; the ruleset keeps the checks declarative and auditable).

* * *

### Recommended rule grouping, scoring, and reporting (manifesto for audit-grade results)

* Grouping
  
  * Group 1 (Critical): ProviderNameProvided; ManifestFileExists; ProviderRegisteredInRegistry; ProviderRegisteredInWevtutil.
  * Group 2 (Consistency): ManifestParsed; ManifestChannelsMatchRegistryBindings; ManifestChannelNameNormalization.
  * Group 3 (Security): ManifestFileAclSecure; RegistryAclsSecure.
  * Group 4 (Integrity & Audit): ManifestInstallMatchesProviderHash; AuditEvidenceAttached.

* Scoring model (example)
  
  * Critical failure (any Group 1 rule false) ⇒ overall status: Failed, severity: High.
  * Any Group 3 failure ⇒ security warning, severity: Medium-High.
  * Group 2 inconsistency ⇒ configuration warning, severity: Medium.
  * Group 4 failure ⇒ integrity warning, severity: Medium.

* Output schema (for each rule)
  
  * **ruleName**, **passed** (bool), **message** (string), **evidence** (attach raw probe outputs), **timestamp**.
  * Store workflow run as an immutable artifact (JSON), signed with probe operator identity and timestamp for future forensic review.

* * *

### Example outputs and evidence attachments

* Always attach:
  * Raw wevtutil query output.
  * Raw registry reads (full key names and values).
  * Manifest file content (or its SHA256) and parsed structure.
  * Raw ACL SDDL or parsed ACE list for both file and registry keys.

This ensures non-repudiable provenance and auditability for every rule result.

* * *

### References and implementation notes

* The RulesEngine library supports JSON-defined workflows and registering custom C# functions for extended evaluation; register the probe helpers described above into the engine before evaluating the workflow.
* EventSource/EventRegister registration and manifest build/install practices are documented in the EventSource docs and the EventRegister tooling notes; use wevtutil and EventRegister outputs for authoritative provider listing when probing installed manifests.
* Manifest ACLs may also be defined by deployment tooling (e.g., manifest SetAcl directives during deployment); consider comparing deployed ACLs against desired state in your manifest-driven deployment pipeline.

Sources:

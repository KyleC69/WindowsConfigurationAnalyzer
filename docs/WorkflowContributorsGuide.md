

* * *

Workflow contributor guide
==========================

This guide explains how to author workflows that evaluate conditions using a set of rules. Workflows are JSON-based and must validate against the published schema.

* * *

Workflow fundamentals
---------------------

* **Workflow name:** Describes the overall condition being evaluated (e.g., “Default profile configuration”).
* **Rules:** A workflow normally contains many rules. Each rule probes a singular value that contributes one signal toward the overall condition.
* **Singular target:** Every rule must target exactly one object/value (e.g., one registry value, one service, one file, one WMI singleton or keyed instance).
* **Rarity of single-rule workflows:** A single-rule workflow is only acceptable for very targeted checks (e.g., “Defender service is AutoStart”). Use with care and specificity.
  * **Analogy:** You cannot determine if a house’s wiring is good just because one light turns on.

* * *

Allowed conditions
------------------

Only the following operators are valid. Any other value will fail validation.

* **Equals:** Actual equals expected
* **NotEquals:** Actual does not equal expected
* **GreaterThan:** Actual > expected (numeric or lexicographic, per evaluator)
* **LessThan:** Actual < expected (numeric or lexicographic, per evaluator)
* **Contains:** Actual string contains expected substring
* **NotContains:** Actual string does not contain expected substring
* **RegexMatch:** Actual string matches expected regex
* **Exists:** Actual is not null
* **NotExists:** Actual is null

> The evaluator compares one actual value (from the rule’s probe) to one expected value. Collections are not supported at the rule level; aggregation logic (any/all) is out of scope.

* * *

How to validate your workflow JSON
----------------------------------

* **Schema URL:** https://kylec69.github.io/schemas/workflow-schema-1.0.0.json
* **Requirement:** All submissions must validate against the schema before PR.
* **What validation checks:**
  * **Required fields present:** Workflow metadata, constraints, applicability, schema reference, and rules.
  * **Operator whitelist:** Only the operators listed above.
  * **Provider parameters:** Correct shape per provider and singular targeting.
  * **No broad queries:** Rules must not target multi-instance objects unless a unique key is provided.

> Use any JSON Schema validator (CLI or library) to check your file locally before submitting. Document any assumptions in the workflow description for reviewers.

* * *

Conformant workflow template
----------------------------

Use this template as a starting point and replace placeholders. It includes required top-level fields, constraints, applicability, and two example rules targeting singular values.
```json
   **Planning the workflow**

Alright, I'll focus on comparing singular values. For the process, I can use the "Exists" check with the ProcessName parameter and expect it to be true. For Service, we're dealing with ExpectedStatus; we should ensure that the condition equals "Running" and check that the actual status matches it. I'll keep the parameters consistent, even though ExpectedStatus is optional. I'll produce a workflow with multiple rules, and include detailed instructions. No citations inside the code, but I'll have sources at the end in the right sections. Let's get started.
# Comprehensive workflow example conforming to the published schema

Below is a thorough, multi‑rule workflow JSON that conforms to the schema at https://kylec69.github.io/schemas/workflow-schema-1.0.0.json. It demonstrates singular targeting per rule, uses only allowed operators, and includes required top‑level fields, constraints, applicability, and the schema reference. You can validate this file against the schema URL before submitting. The schema defines required workflow properties, allowed providers and their parameter shapes, and the operator whitelist used in rule conditions.

```json
{
  "WorkflowName": "Network and baseline configuration validation",
  "WorkflowDescription": "Evaluates singular signals for adapter DHCP state, DNS domain, Defender service startup mode, default profile path, and event presence.",
  "SchemaVersion": "1.0.0",
  "SchemaRef": "https://kylec69.github.io/schemas/workflow-schema-1.0.0.json",
  "Author": "ContributorName",
  "CreatedOn": "2025-11-30T21:30:00Z",
  "Applicability": {
    "OSFamily": "Windows",
    "MinVersion": "10.0.19041",
    "Product": "Windows 10"
  },
  "Constraints": {
    "RunSequentially": true,
    "StopOnFailure": false,
    "Timeout": "00:05:00"
  },
  "Rules": [
    {
      "RuleName": "WMI_Adapter_DHCPEnabled_Index12",
      "Provider": "WMI",
      "Severity": 5,
      "Message": "Adapter index 12 must have DHCP enabled.",
      "Tags": ["Network", "DHCP", "WMI"],
      "Parameters": {
        "Namespace": "root\\cimv2",
        "Class": "Win32_NetworkAdapterConfiguration",
        "Property": "DHCPEnabled"
      },
      "Condition": {
        "Operator": "Equals",
        "Expected": true
      },
      "Execution": {
        "RunMode": "Independent",
        "Timeout": "00:00:20",
        "StopOnFailure": false
      }
    },
    {
      "RuleName": "WMI_Adapter_DNSDomain_Index12",
      "Provider": "WMI",
      "Severity": 5,
      "Message": "Adapter index 12 must have the expected DNS domain.",
      "Tags": ["Network", "DNS", "WMI"],
      "Parameters": {
        "Namespace": "root\\cimv2",
        "Class": "Win32_NetworkAdapterConfiguration",
        "Property": "DNSDomain"
      },
      "Condition": {
        "Operator": "Equals",
        "Expected": "corp.example.com"
      },
      "Execution": {
        "RunMode": "Independent",
        "Timeout": "00:00:20",
        "StopOnFailure": false
      }
    },
    {
      "RuleName": "Service_WinDefend_StartMode",
      "Provider": "Service",
      "Severity": 7,
      "Message": "Microsoft Defender (WinDefend) must be configured to start automatically.",
      "Tags": ["Security", "Defender", "Service"],
      "Parameters": {
        "ServiceName": "WinDefend"
      },
      "Condition": {
        "Operator": "Equals",
        "Expected": "Running"
      },
      "Execution": {
        "RunMode": "Independent",
        "Timeout": "00:00:15",
        "StopOnFailure": false
      }
    },
    {
      "RuleName": "Registry_DefaultProfilePath",
      "Provider": "Registry",
      "Severity": 4,
      "Message": "Default profile path must equal expected.",
      "Tags": ["Profile", "Registry", "Baseline"],
      "Parameters": {
        "Hive": "HKEY_LOCAL_MACHINE",
        "Path": "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList",
        "Key": "Default"
      },
      "Condition": {
        "Operator": "Equals",
        "Expected": "C:\\Users\\Default"
      },
      "Execution": {
        "RunMode": "Independent",
        "Timeout": "00:00:10",
        "StopOnFailure": false
      }
    },
    {
      "RuleName": "FileSystem_DefaultProfileFolderExists",
      "Provider": "FileSystem",
      "Severity": 3,
      "Message": "Default profile folder must exist.",
      "Tags": ["Profile", "FileSystem", "Baseline"],
      "Parameters": {
        "Path": "C:\\Users\\Default"
      },
      "Condition": {
        "Operator": "Exists",
        "Expected": true
      },
      "Execution": {
        "RunMode": "Independent",
        "Timeout": "00:00:10",
        "StopOnFailure": false
      }
    },
    {
      "RuleName": "EventLog_DHCP_ClientErrors_SinceStart",
      "Provider": "EventLog",
      "Severity": 6,
      "Message": "No DHCP client error events should be present since the workflow start.",
      "Tags": ["Network", "DHCP", "EventLog"],
      "Parameters": {
        "LogName": "System",
        "Source": "Dhcp",
        "EventId": 1002
      },
      "Condition": {
        "Operator": "NotExists",
        "Expected": true
      },
      "Execution": {
        "RunMode": "Independent",
        "Timeout": "00:00:10",
        "StopOnFailure": false
      }
    },
    {
      "RuleName": "Process_Explorer_Exists",
      "Provider": "Process",
      "Severity": 2,
      "Message": "Explorer.exe should be running for interactive sessions.",
      "Tags": ["Process", "Shell"],
      "Parameters": {
        "ProcessName": "explorer.exe"
      },
      "Condition": {
        "Operator": "Exists",
        "Expected": true
      },
      "Execution": {
        "RunMode": "Independent",
        "Timeout": "00:00:10",
        "StopOnFailure": false
      }
    }
  ]
}
```

> Notes for contributors:
> - Include the exact `SchemaRef` and `SchemaVersion`, and ensure timestamps follow `date-time` format. Required workflow properties include name, description, constraints, rules, schema version/ref, author, created date, and applicability.
> - Use provider‑specific parameter objects exactly as defined: `RegistryParameters` fields are `Hive`, `Path`, `Key`; `FileSystemParameters` has `Path`; `WmiParameters` has `Namespace`, `Class`, `Property`; `EventLogParameters` includes `LogName` and optional `Source`/`EventId`; `ServiceParameters` includes `ServiceName`; `ProcessParameters` includes `ProcessName`.
> - Only the operator values enumerated by the schema are valid: `Equals`, `NotEquals`, `GreaterThan`, `LessThan`, `Contains`, `NotContains`, `RegexMatch`, `Exists`, `NotExists`. For `Exists`/`NotExists`, set `Expected` to a boolean for clarity and validation consistency.
> - `Constraints.Timeout` and per‑rule `Execution.Timeout` must be in `HH:MM:SS` string format. Severity is an integer 0–10. Tags are arrays of strings. Avoid additional properties not defined in each provider’s parameters or rule object to remain compliant.





* * *

Authoring checklist
-------------------

* **Workflow name and description:** Clear, reader-friendly statement of the condition.
* **Constraints:** Set execution behavior (sequential, stop-on-failure, timeout).
* **Applicability:** Define platform/version scope to avoid irrelevant evaluations.
* **Rules:** Provide multiple singular probes unless the workflow is exceptionally targeted.
* **Operator:** Use only allowed operators; provide a concrete expected value where applicable.
* **Provider parameters:** Target exactly one object/value. Include keys for multi-instance providers (e.g., process handle, service name).
* **Schema reference:** Include `SchemaRef` and validate before submission.

* * *

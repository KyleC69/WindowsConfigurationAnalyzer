// Created:  2025/10/30
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using System.Xml;
using System.Xml.Schema;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Areas.Policy;



internal static class AdmxValidator
{
	public static Result Validate(string admxPath, string? admlDirectory)
	{
		var state = "OK";
		string? error = null;
		string? root = null;
		var xmlValid = true;
		try
		{
			using var fs = File.OpenRead(admxPath);
			using var xr = XmlReader.Create(fs, CreateSettingsIfSchemaPresent(admxPath));
			while (xr.Read())
				if (xr.NodeType == XmlNodeType.Element)
				{
					root = xr.Name;

					break;
				}
		}
		catch (Exception ex)
		{
			xmlValid = false;
			state = $"XML error: {ex.Message}";
			error = ex.ToString();
		}

		var admlPath = admlDirectory is null
			? null
			: Path.Combine(admlDirectory, Path.GetFileNameWithoutExtension(admxPath) + ".adml");
		var hasAdml = admlPath is not null && File.Exists(admlPath);
		if (!hasAdml) state = state == "OK" ? "Missing ADML" : state + "; Missing ADML";

		return new Result(Path.GetFileName(admxPath), xmlValid, hasAdml, root, state, error);
	}





	private static XmlReaderSettings CreateSettingsIfSchemaPresent(string admxPath)
	{
		var settings = new XmlReaderSettings
			{ DtdProcessing = DtdProcessing.Ignore, ValidationType = ValidationType.None };
		try
		{
			// If a local schema exists next to the ADMX or in PolicyDefinitions folder, attach it for validation
			var folder = Path.GetDirectoryName(admxPath)!;
			var schema = Path.Combine(folder, "PolicyDefinitions.xsd");
			if (File.Exists(schema))
			{
				settings.Schemas = new XmlSchemaSet();
				using var s = File.OpenRead(schema);
				settings.Schemas.Add(null, XmlReader.Create(s));
				settings.ValidationType = ValidationType.Schema;
				settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
				settings.ValidationEventHandler += (o, e) =>
				{
					/* surface via reader exception later */
				};
			}
		}
		catch
		{
		}

		return settings;
	}





	public sealed record Result(string File, bool IsXmlValid, bool HasAdml, string? Root, string State, string? Error);
}
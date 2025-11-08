// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Contracts;
using KC.WindowsConfigurationAnalyzer.Analyzer.Core.Models;
using Microsoft.Extensions.Logging;



namespace KC.WindowsConfigurationAnalyzer.Analyzer.Core.Infrastructure;



public interface ITimeProvider
{
	DateTimeOffset UtcNow { get; }
}




public interface IActionLogFile
{
	void Append(string line);
}




public sealed class FileActionLogSink : IActionLogFile
{
	private readonly object _gate = new();
	private readonly string _path;





	public FileActionLogSink(string path)
	{
		_path = path;
	}





	public void Append(string line)
	{
		try
		{
			lock (_gate)
			{
				File.AppendAllText(_path, line + Environment.NewLine);
			}
		}
		catch
		{
			// swallow: file logging must never break analysis
		}
	}
}




public sealed class SystemTimeProvider : ITimeProvider
{
	public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}




/// <summary>
///     Emits structured log entries to ILogger, single-line raw file log, and manifest-based ETW via IEventProvider.
/// </summary>
public sealed class ActionLogger
{
	private readonly Dictionary<string, int> _areaSeq = new(StringComparer.OrdinalIgnoreCase);
	private readonly List<ActionLogEntry> _entries = [];
	private readonly IEventProvider? _eventProvider;
	private readonly IActionLogFile? _fileSink;
	private readonly object _gate = new();
	private readonly ILogger _logger;





	public ActionLogger(ILogger logger, IActionLogFile? fileSink = null, IEventProvider? eventProvider = null)
	{
		_logger = logger;
		_fileSink = fileSink;
		_eventProvider = eventProvider;
	}





	public IReadOnlyList<ActionLogEntry> Entries
	{
		get
		{
			lock (_gate)
			{
				return _entries.ToList();
			}
		}
	}





	public void Info(string area, string action, string message)
	{
		Write(area, action, "Information", message, null, (lvl, msg) => _logger.LogInformation("{Message}", msg));
	}





	public void Warn(string area, string action, string message)
	{
		Write(area, action, "Warning", message, null, (lvl, msg) => _logger.LogWarning("{Message}", msg));
	}





	public void Error(string area, string action, string message, Exception ex)
	{
		Write(area, action, "Error", message, ex.ToString(), (lvl, msg) => _logger.LogError(ex, "{Message}", msg));
	}





	private void Write(string area, string action, string level, string message, string? exception,
		Action<string, string> sink)
	{
		var ts = DateTimeOffset.UtcNow;
		var entry = new ActionLogEntry(ts, area, action, level, message, exception);
		lock (_gate)
		{
			_entries.Add(entry);
		}

		// .NET structured logging
		sink(level, message);

		// Raw file: single line, ISO8601 UTC, tab-delimited
		_fileSink?.Append(
			$"{ts:O}\t{area}\t{action}\t{level}\t{message}{(exception is null ? string.Empty : "\t" + exception)}");

		// Manifest-based ETW via abstraction; map to taxonomy by level
		try
		{
			var seq = NextSeq(area);
			_eventProvider?.EmitAction(area, action, level, message, exception, seq);
		}
		catch
		{
			// never let ETW issues break analysis
		}
	}





	private int NextSeq(string area)
	{
		lock (_gate)
		{
			if (!_areaSeq.TryGetValue(area, out var v)) v = -1;

			v = (v + 1) % 100;
			_areaSeq[area] = v;

			return v;
		}
	}
}
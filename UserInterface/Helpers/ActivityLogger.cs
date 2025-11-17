// Created:  2025/11/09
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  ActivityLogger.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder




using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

using KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;
using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Etw;





namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;


/// <summary>
///     High-frequency, low-overhead activity logger intended for early initialization and diagnostic tracing.
///     Enabled by default until <see cref="InitializeAsync(ILocalSettingsService?)" /> is called and disables it via
///     settings.
///     This logger is buffered, bounded, and resilient: it never throws from logging calls and will drop entries when
///     overloaded.
/// </summary>
public static class ActivityLogger
{


    // Protects pending count; allows quick drop when overloaded
    private const int MaxPending = 50_000; // upper bound to avoid unbounded memory use

    // Default: enabled until settings service says otherwise
    private static volatile bool _isEnabled = true;

    private static readonly string LogDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "KC.WindowsConfigurationAnalyzer", "ActivityLogs");

    private static readonly ConcurrentQueue<string> _queue = new();
    private static StreamWriter? _writer;
    private static CancellationTokenSource? _cts;
    private static Task? _worker;
    private static readonly object _initLock = new();
    private static bool _initialized;
    private static int _pendingCount;





    // Start background worker and try to create writer eagerly so logging is available as early as possible
    static ActivityLogger()
    {
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(LogDirectory);
            Debug.Write(LogDirectory);

            // Create writer lazily but start worker which will attempt to create writer on demand
            _cts = new CancellationTokenSource();
            _worker = Task.Run(() => ProcessQueueAsync(_cts.Token));

            // mark initialized so Log can work with default-enabled behaviour until settings arrive
            _initialized = true;
        }
        catch (Exception ex)
        {
            // Never crash application for logging errors
            _isEnabled = false;
            _initialized = true;
            LogFallback($"Logger static init failed: {ex.Message}");
        }
    }










    /// <summary>
    ///     Initialize logger with settings service. If settings explicitly disable logging (value "false"), logger will be
    ///     turned off.
    ///     Call this once DI is available; logger is enabled by default until this call completes.
    /// </summary>
    /// <param name="localSettingsService">The settings service used to read persisted flags. If null, no changes are made.</param>
    public static async Task InitializeAsync(ILocalSettingsService? localSettingsService)
    {
        if (localSettingsService is null)
        {
            return; // nothing to do - keep default behaviour
        }

        try
        {
            var raw = await localSettingsService.ReadApplicationSettingAsync<string>("IsActivityLoggingEnabled")
                .ConfigureAwait(false);
            if (string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase))
            {
                // Turn off logging if settings say so
                DisableLogging();

                return;
            }

            // If enabled by setting, ensure writer is ready
            EnsureWriter();
        }
        catch (Exception ex)
        {
            // Reading settings failed — keep default enabled state but do not throw
            LogFallback($"Logger InitializeAsync failed to read settings: {ex.Message}");
            EnsureWriter();
        }
    }





    /// <summary>
    ///     Explicitly disable logging. This will stop the background worker and attempt to flush queued entries.
    ///     Safe to call multiple times.
    /// </summary>
    public static void DisableLogging()
    {
        lock (_initLock)
        {
            if (!_isEnabled)
            {
                return;
            }

            _isEnabled = false;
        }

        // Stop worker and flush
        try
        {
            _cts?.Cancel();

            // Drain remaining entries without throwing
            while (_queue.TryDequeue(out var line))
            {
                try
                {
                    EnsureWriter();
                    _writer?.WriteLine(line);
                }
                catch
                {
                    /* swallow */
                }
            }

            try
            {
                _writer?.Flush();
            }
            catch
            {
                /* swallow */
            }

            try
            {
                _writer?.Dispose();
            }
            catch
            {
                /* swallow */
            }
        }
        catch
        {
            /* swallow */
        }
        finally
        {
            _writer = null;
            _cts?.Dispose();
            _cts = null;
            _worker = null;
        }
    }





    /// <summary>
    ///     Lightweight synchronous initializer when caller already knows enabled state and wants early init.
    /// </summary>
    /// <param name="enabled">True to enable logging; false to disable.</param>
    public static void Initialize(bool enabled)
    {
        lock (_initLock)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _isEnabled = enabled;
        }

        if (_isEnabled)
        {
            EnsureWriter();
        }
    }





    /// <summary>
    ///     Enqueue a log entry. This method is designed to be very low overhead and will not block the caller.
    ///     If the internal queue is full, entries will be dropped to protect application stability.
    /// </summary>
    /// <param name="level">A short level string (e.g. "INF", "ERR").</param>
    /// <param name="message">The log message.</param>
    /// <param name="context">Optional context string.</param>
    public static void Log(string level, string message, string context)
    {
        //   App.LogCounter.Increment();

        // Fast check
        if (!_isEnabled || !_initialized)
        {
            return;
        }

        try
        {
            // Build CSV line with minimal allocations
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var line = $"{Escape(timestamp)},{Escape(level)},{Escape(message)},{Escape(context)}";

            // Bounded enqueue using atomic counter to avoid using Count on ConcurrentQueue
            if (Interlocked.Increment(ref _pendingCount) > MaxPending)
            {
                // drop this entry to protect app memory
                Interlocked.Decrement(ref _pendingCount);

                return;
            }

            _queue.Enqueue(line);
        }
        catch (Exception ex)
        {
            // Never throw from logging
            try
            {
                WCAEventSource.Log.ActivityLoggerFallback($"LOGGING FAILED: {ex.Message}");
                LogFallback($"Log enqueue failed: {ex.Message}");
            }
            catch
            {
                /* swallow */
            }
        }
    }





    public static void Info(string context, string action, string message)
    {
        Log("INF", $"{action}: {message}", context);
    }





    public static void Error(string context, string action, string message, Exception? ex)
    {
        Log("ERR", $"{action}: {message}", context);
        //TODO: Also send to event log!
    }





    public static void Warn(string context, string action, string message)
    {
        Log("WRN", $"{action}: {message}", context);
        //TODO: Also send to event log!
    }





    /// <summary>
    ///     Flush queued entries to disk. Best-effort and non-blocking.
    /// </summary>
    public static void Flush()
    {
        try
        {
            // Wait briefly for worker to drain
            var sw = Stopwatch.StartNew();
            while (_pendingCount > 0 && sw.Elapsed < TimeSpan.FromSeconds(2))
            {
                Thread.Sleep(50);
            }

            try
            {
                _writer?.Flush();
            }
            catch
            {
                /* swallow */
            }
        }
        catch
        {
            /* swallow */
        }
    }





    /// <summary>
    ///     Shutdown logger and flush. Call during application exit.
    /// </summary>
    public static void Shutdown()
    {
        try
        {
            _cts?.Cancel();

            if (_worker != null)
            {
                try
                {
                    _worker.Wait(2000);
                }
                catch
                {
                    /* swallow */
                }
            }

            while (_queue.TryDequeue(out var line))
            {
                try
                {
                    EnsureWriter();
                    _writer?.WriteLine(line);
                }
                catch
                {
                    /* swallow */
                }
            }

            try
            {
                _writer?.Flush();
            }
            catch
            {
                /* swallow */
            }

            try
            {
                _writer?.Dispose();
            }
            catch
            {
                /* swallow */
            }
        }
        catch
        {
            /* swallow */
        }
        finally
        {
            _writer = null;
            _cts?.Dispose();
            _cts = null;
            _worker = null;
            _isEnabled = false;
            _initialized = false;
        }
    }





    private static void EnsureWriter()
    {
        if (!_isEnabled)
        {
            return;
        }

        if (_writer != null)
        {
            return;
        }

        lock (_initLock)
        {
            if (_writer != null)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(LogDirectory);
                var fileName = $"ActivityLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var logFilePath = Path.Combine(LogDirectory, fileName);

                var fs = new FileStream(logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(fs, Encoding.UTF8) { AutoFlush = false };
                _writer.WriteLine("Timestamp,Level,Message,Context");
            }
            catch (Exception ex)
            {
                // If writer cannot be created, disable logging to avoid repeated errors
                _isEnabled = false;
                LogFallback($"EnsureWriter failed: {ex.Message}");
            }
        }
    }





    private static async Task ProcessQueueAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // Attempt to create writer if absent and logging is enabled
                if (_isEnabled)
                {
                    EnsureWriter();
                }

                // Dequeue and write as many as available
                while (_queue.TryDequeue(out var line))
                {
                    Interlocked.Decrement(ref _pendingCount);

                    try
                    {
                        if (_writer != null)
                        {
                            await _writer.WriteLineAsync(line);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Swallow write exceptions but record fallback
                        try
                        {
                            LogFallback($"Log write failed: {ex.Message}");
                        }
                        catch
                        {
                            /* swallow */
                        }
                    }
                }

                // Ensure data is flushed to disk after processing available items to reduce truncation on abrupt exit
                try
                {
                    await _writer?.FlushAsync(ct)!;
                }
                catch
                {
                    /* swallow */
                }

                // Batch interval
                await Task.Delay(200, ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            /* expected on shutdown */
        }
        catch (Exception ex)
        {
            try
            {
                LogFallback($"Logging worker failed: {ex.Message}");
            }
            catch
            {
                /* swallow */
            }
        }
    }





    private static string Escape(string? input)
    {
        return string.IsNullOrEmpty(input) ? string.Empty : input.Contains(",") ? $"\"{input.Replace("\"", "\"\"")}\"" : input;
    }





    private static void LogFallback(string error)
    {
        try
        {
            WCAEventSource.Log.ActivityLoggerFallback(error);
            EventLog.WriteEntry("Application", error, EventLogEntryType.Warning);
        }
        catch
        {
            /* swallow */
        }
    }


}
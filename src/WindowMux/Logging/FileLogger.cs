using Microsoft.Extensions.Logging;

namespace ScottReece.WindowMux.Logging;

/// <summary>
/// Simple file-based logger for debugging.
/// </summary>
public sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logFilePath;
    private readonly LogLevel _minLevel;
    private static readonly object _lock = new();

    public FileLogger(string categoryName, string logFilePath, LogLevel minLevel)
    {
        _categoryName = categoryName;
        _logFilePath = logFilePath;
        _minLevel = minLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {_categoryName}: {message}";

        if (exception != null)
        {
            logEntry += Environment.NewLine + exception;
        }

        lock (_lock)
        {
            try
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}

/// <summary>
/// Provider for FileLogger instances.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;
    private readonly LogLevel _minLevel;

    public FileLoggerProvider(string logFilePath, LogLevel minLevel = LogLevel.Debug)
    {
        _logFilePath = logFilePath;
        _minLevel = minLevel;

        // Ensure directory exists
        var directory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Truncate log file on startup if too large
        try
        {
            if (File.Exists(logFilePath))
            {
                var info = new FileInfo(logFilePath);
                if (info.Length > 10 * 1024 * 1024) // 10MB
                {
                    File.Delete(logFilePath);
                }
            }
        }
        catch
        {
            // Ignore
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _logFilePath, _minLevel);
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}

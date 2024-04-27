using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using McRider.Common.Helpers;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace McRider.Common.Logging;

public class CustomLoggerProvider : ILoggerProvider
{
    private readonly string _configFile; // config File
    private readonly Func<IDictionary<string, object>> _getLogData;

    public CustomLoggerProvider(string configFile, Func<IDictionary<string, object>> getLogData)
    {
        _configFile = configFile;
        _getLogData = getLogData;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new CustomLogger(_configFile, categoryName, _getLogData, LogLevel.Trace);
    }

    public void Dispose()
    {
        // Cleanup logic, if any
    }
}

public class CustomLogger : ILogger
{
    private readonly NLog.Logger _logger;
    private readonly string? _categoryName;
    private readonly LogLevel _minLogLevel;
    private readonly Func<IDictionary<string, object>> _getLogData;

    public CustomLogger(string configFile, string categoryName, Func<IDictionary<string, object>> getLogData, LogLevel minLogLevel)
    {
        _categoryName = categoryName?.Split('.')?.LastOrDefault();
        _minLogLevel = minLogLevel;
        _getLogData = getLogData;

        var assembly = AssemblyHelpers.GetAssemblyForResource(configFile);
        if (assembly == null)
        {
            configFile = Regex.Replace(configFile, @".(\w*).config", ".config");
            assembly = AssemblyHelpers.GetAssemblyForResource(configFile);
        }

        if (assembly != null)
        {
            var stream = assembly.GetManifestResourceStream(configFile);
            if (stream != null)
                NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(XmlReader.Create(stream), null);

#if DEBUG
            // Log everything in debug mode
            if (NLog.LogManager.Configuration?.LoggingRules != null)
            {
                foreach (var r in NLog.LogManager.Configuration.LoggingRules)
                {
                    r.EnableLoggingForLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
                }
            }
#endif
        }

        _logger = NLog.LogManager.GetLogger(_categoryName);
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        // You can implement scope logic if needed
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // Check if the log level is enabled
        return logLevel >= _minLogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        // Check if the log level is enabled
        if (!IsEnabled(logLevel)) return;

        // Perform custom logging logic here
        string message = formatter(state, exception);

        // Example: Print the log message to the console
        Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{Environment.CurrentManagedThreadId:00}] [{logLevel}] {_categoryName}: {message}");

        // Customize this part to log to use NLog
        var levelName = logLevel.ToString();
        var level = NLog.LogLevel.AllLevels?.FirstOrDefault(x => x != null && levelName.StartsWith(x.Name));
        if (level == null && logLevel == LogLevel.Critical)
            level = NLog.LogLevel.Fatal;
        level ??= NLog.LogLevel.AllLevels?.FirstOrDefault();

        var data = _getLogData?.Invoke() ?? new Dictionary<string, object>();

        data["@timestamp"] = DateTime.UtcNow.ToString("s");
        data["host"] = Environment.MachineName;
        data["Message"] = message;
        data["Level"] = (level?.Name ?? levelName).ToUpper();
        data["StackTrace"] = exception?.StackTrace;
        data["Exception"] = exception?.GetDetails();

        _logger.Log(level, JsonConvert.SerializeObject(data));
    }
}

public static class ExeptionExtensions
{
    /// <summary>
    /// Get a string display of a specific exception
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static string GetDetails(this Exception ex)
    {
        var properties = ex.GetType().GetProperties();
        var fields = new List<string>();

        foreach (PropertyInfo property in properties)
        {
            if (property.Name == "Message") continue;

            try
            {
                object value = property.GetValue(ex, null);
                if (!string.IsNullOrEmpty(value?.ToString()))
                    fields.Add(String.Format("\t{0} = {1}", property.Name, value?.ToString() ?? String.Empty));
            }
            catch
            {
                /* Eat the Exception */
            }
        }

        var msg = string.Empty;

        if (ex.InnerException != null)
        {
            msg += ex.InnerException?.GetDetails();
            msg += "\n-------------------------------------\n";
        }

        msg += $"\n{ex.Message}\n\t" + String.Join(",\n\t", fields.ToArray());

        return msg;
    }
}
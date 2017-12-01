using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonolithicExtensions.Portable.Logging
{
    /// <summary>
    /// Represents an object used for logging. There are various methods which output messages
    /// depending on the logger given.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Each of the handlers in this logger. A handler is the thing that actually outputs messages
        /// </summary>
        IList<ILogHandler> Handlers { get; set; }
        string Name { get; set; }

        void Initialize(string name);

        void Trace(string message);
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Fatal(string message);
        void LogRaw(string message, int level);
    }

    /// <summary>
    /// A handler for logging. Performs the actual message output (and potentially formatting)
    /// based on the given settings.
    /// </summary>
    public interface ILogHandler
    {
        LogHandlerSettings Settings { get; set; }

        void LogRaw(string message, int level, string name);
    }

    /// <summary>
    /// Settings for each log handler 
    /// </summary>
    public class LogHandlerSettings
    {
        public int MinLogLevel = (int)LogLevel.Debug;
        public string Format = $"{LogFormatIdentifiers.LevelAligned} [{LogFormatIdentifiers.TimePrecise}] {LogFormatIdentifiers.Message} [{LogFormatIdentifiers.Name}:{LogFormatIdentifiers.DateSimple}]";
        public bool HandleFormat = true;
        public bool HandleLevel = true;
    }

    /// <summary>
    /// The various levels reported by the logger.
    /// </summary>
    public enum LogLevel
    {
        Fatal = 6000,
        Error = 5000,
        Warn = 4000,
        Info = 3000,
        Debug = 2000,
        Trace = 1000
    }

    /// <summary>
    /// The various identifiers you can use in the log format
    /// </summary>
    public static class LogFormatIdentifiers
    {
        public const string Message = "{0}";
        public const string Level = "{1}";
        public const string LevelAligned = "{1,-6}";
        public const string TimeSimple = "{2}";
        public const string TimePrecise = "{3}";
        public const string DateSimple = "{4}";
        public const string DatePrecise = "{5}";
        public const string Name = "{6}";
    }
}

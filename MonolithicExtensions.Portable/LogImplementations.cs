using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonolithicExtensions.Portable.Logging
{
    public class BasicLogger : ILogger
    {
        public IList<ILogHandler> Handlers { get; set; } = new List<ILogHandler>();
        public string Name { get; set; } = "";

        public void Initialize(string name)
        {
            this.Name = name;
        }

        public void Debug(string message) { LogRaw(message, (int)LogLevel.Debug); }
        public void Error(string message) { LogRaw(message, (int)LogLevel.Error); }
        public void Fatal(string message) { LogRaw(message, (int)LogLevel.Fatal); }
        public void Info(string message) { LogRaw(message, (int)LogLevel.Info); }
        public void Trace(string message) { LogRaw(message, (int)LogLevel.Trace); }
        public void Warn(string message) { LogRaw(message, (int)LogLevel.Warn); }

        public void LogRaw(string message, int level)
        {
            foreach (ILogHandler handler in Handlers)
            {
                //Skip over messages that are too low level to log with this handler.
                if (handler.Settings.HandleLevel && level < handler.Settings.MinLogLevel) continue;

                if(handler.Settings.HandleFormat)
                {
                    string outputLevel = level.ToString();
                    DateTime now = DateTime.Now;
                    if (Enum.IsDefined(typeof(LogLevel), level)) outputLevel = ((LogLevel)level).ToString("G");

                    message = String.Format(handler.Settings.Format, message, outputLevel, now.ToString("HHmm.ss"), now.ToString("HHmm.ss,fff"), now.ToString("yyMMdd"), now.ToString("D"), Name);
                }

                handler.LogRaw(message, level, Name);
            }
        }
    }

    /// <summary>
    /// A logger that simply acts as proxy for the real logger implemented in LogServices.DefaultLogger. This
    /// way, if someone requests a logger that's a "copy" of the default logger, they get just that. Changes
    /// to the default logger will be reflected in all copies.
    /// </summary>
    public class DefaultLoggerProxy : ILogger
    {
        public IList<ILogHandler> Handlers
        {
            get { return LogServices.DefaultLogger.Handlers; }
            set { LogServices.DefaultLogger.Handlers = value; }
        } 

        /// <summary>
        /// Even though this is just a proxy, it can have its own name.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Default logger will NOT be reinitialized. Assume it has been done already.
        /// </summary>
        /// <param name="name"></param>
        public void Initialize(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>This could be VERY performance heavy. Use with caution.</remarks>
        /// <param name="task"></param>
        private void RunWithName(Action task)
        {
            var defaultName = LogServices.DefaultLogger.Name;
            LogServices.DefaultLogger.Name = Name;
            task.Invoke();
            LogServices.DefaultLogger.Name = defaultName;
        }

        public void Debug(string message) { RunWithName(() => LogServices.DefaultLogger.Debug(message)); }
        public void Error(string message) { RunWithName(() => LogServices.DefaultLogger.Error(message)); }
        public void Fatal(string message) { RunWithName(() => LogServices.DefaultLogger.Fatal(message)); }
        public void Info(string message) { RunWithName(() => LogServices.DefaultLogger.Info(message)); }
        public void Trace(string message) { RunWithName(() => LogServices.DefaultLogger.Trace(message)); }
        public void Warn(string message) { RunWithName(() => LogServices.DefaultLogger.Warn(message)); }
        public void LogRaw(string message, int level) { RunWithName(() => LogServices.DefaultLogger.LogRaw(message, level)); }
    }

    /// <summary>
    /// Because of the special circumstances for logging, this LogServices is a different kind of 
    /// DIFactory. It does not directly inherit from it, but it has settable methods for creating
    /// the logger.
    /// </summary>
    public static class LogServices
    {
        /// <summary>
        /// The logger from which most generic loggers (handed out to objects) are copied from.
        /// </summary>
        public static ILogger DefaultLogger = null;

        /// <summary>
        /// The factory that describes how to create loggers for all objects that request one.
        /// </summary>
        public static DIFactory LogCreator;  //= new DIFactory();

        /// <summary>
        /// This ensures that an unitialized service is at least still valid and doesn't crash.
        /// </summary>
        static LogServices()
        {
            LogCreator = new DIFactory(false);
            Initialize(x => new BasicLogger());
        }

        /// <summary>
        /// Initialize log services, preferably before you start creating log objects. Optionally, you can
        /// indicate how you want to create loggers.
        /// </summary>
        public static void Initialize(Func<DIFactory, ILogger> creator = null, bool recreateDefaultLogger = true)
        {
            if(creator != null)
            {
                if (!LogCreator.CreationMapping.ContainsKey(typeof(ILogger)))
                    LogCreator.CreationMapping.Add(typeof(ILogger), creator);
                else
                    LogCreator.CreationMapping[typeof(ILogger)] = creator;
            }

            if (recreateDefaultLogger || DefaultLogger == null)
            {
                DefaultLogger = LogCreator.Create<ILogger>();
                DefaultLogger.Initialize("DefaultLogger");
            }
        }

        /// <summary>
        /// Create a copy of the DefaultLogger which will use the same handlers and settings.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ILogger CreateLoggerFromDefault(string name)
        {
            ILogger result = new DefaultLoggerProxy(); //'LogCreator.Create<ILogger>(); 
            result.Initialize(name);
            result.Name = name;
            return result;
            //result.Handlers = DefaultLogger.Handlers;
        }

        /// <summary>
        /// Use the object type as the name for the logger to be created as a copy and reference to the default logger.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static ILogger CreateLoggerFromDefault(Type objectType)
        {
            return CreateLoggerFromDefault(objectType.Namespace + "." + objectType.Name);
        }
    }
}

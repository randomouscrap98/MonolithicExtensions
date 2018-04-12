using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using log4net;
using System.Xml;
using MonolithicExtensions.Portable.Logging;

namespace MonolithicExtensions.General.Logging
{
    /// <summary>
    /// A logger which is a wrapper for log4net. It turns the MonolithicExtensions.Portable.Logging.ILogger logger into 
    /// the log4net logger (basically).
    /// </summary>
    public class Log4NetWrapper : ILogger
    {
        private log4net.ILog Logger;

        //Later, this should at least attempt to convert the appenders to handlers... maybe?
        public IList<ILogHandler> Handlers
        {
            get { return null; }
            set { /* do nothing */ }
        }

        //Later, you may want to throw an exception when you attempt to set the Name
        public string Name
        {
            get { return Logger.Logger.Name; }
            set { Logger = log4net.LogManager.GetLogger(value); }
        }

        public void Initialize(string name)
        {
            Name = name;
        }

        public log4net.Core.Level ConvertToLog4NetLevel(int level)
        {
            try
            {
                switch ((LogLevel)level)
                {
                    case LogLevel.Debug:
                        return log4net.Core.Level.Debug;
                    case LogLevel.Error:
                        return log4net.Core.Level.Error;
                    case LogLevel.Fatal:
                        return log4net.Core.Level.Fatal;
                    case LogLevel.Info:
                        return log4net.Core.Level.Info;
                    case LogLevel.Trace:
                        return log4net.Core.Level.Trace;
                    case LogLevel.Warn:
                        return log4net.Core.Level.Warn;
                    default:
                        throw new InvalidOperationException("There is no log4net level for " + level);
                }
            }
            catch (Exception ex)
            {
                //Potential for infinite recursion IF LogLevel.Error can't be converted to log4net
                LogRaw($"Could not convert level {level} to log4net level: {ex}", (int)LogLevel.Error);
                return log4net.Core.Level.Info;
            }
        }

        public void Debug(string message) { Logger.Debug(message); }
        public void Error(string message) { Logger.Error(message); }
        public void Fatal(string message) { Logger.Fatal(message); }
        public void Info(string message) { Logger.Info(message); }
        public void Trace(string message) { Logger.Trace(message); }
        public void Warn(string message) { Logger.Warn(message); }

        public void LogRaw(string message, int level)
        {
            Logger.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, ConvertToLog4NetLevel(level), message, null);
        }
    }

    /// <summary>
    /// Contains functions for configuring the log services to use Log4net.
    /// </summary>
    public static class Log4NetExtensions 
    {
        public static void Trace(this ILog Logger, string Message, Exception Ex = null)
        {
            Logger.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, log4net.Core.Level.Trace, Message, Ex);
        }

        /// <summary>
        /// A preconfigured log4net configuration which writes to a local file named "LogName"_log.txt (where LogName is a log4net property).
        /// The files can be up to 1MB in size and can keep 10 rollover files. All messages up to TRACE are logged.
        /// </summary>
        /// <returns></returns>
        public static XElement DebugLoggerConfig = XElement.Parse(@"
        <log4net>
            <appender name=""RollingFileAppender"" type=""log4net.Appender.RollingFileAppender"">
                <file type=""log4net.Util.PatternString"">
                    <conversionPattern value=""%property{LogName}_log.txt""/>
                </file>
                <appendToFile value=""true""/>
                <rollingStyle value=""Size""/>
                <maxSizeRollBackups value = ""10""/>
                <maximumFileSize value=""1MB""/>
                <staticLogFileName value=""true""/>
                <layout type=""log4net.Layout.PatternLayout"">
                    <conversionPattern value=""%-5level [%date{HHmm.ss,ffff}][%thread] - %message - [%date{MM-dd-yyyy} %logger:%line]%newline""/>
                </layout>
            </appender>
            <appender name=""ColoredConsoleAppender"" type=""log4net.Appender.ColoredConsoleAppender"">
                <mapping>
                    <level value=""FATAL""/>
                    <foreColor value=""Blue""/>
                    <backColor value=""Red, HighIntensity""/>
                </mapping>
                <mapping>
                    <level value=""ERROR""/>
                    <foreColor value=""Red""/>
                </mapping>
                <mapping>
                    <level value=""WARN""/>
                    <foreColor value=""Yellow""/>
                </mapping>
                <mapping>
                    <level value=""INFO""/>
                    <foreColor value=""White""/>
                </mapping>
                <mapping>
                    <level value=""DEBUG""/>
                    <foreColor value=""Green""/>
                </mapping>
                <mapping>
                    <level value=""TRACE""/>
                    <foreColor value=""Cyan""/>
                </mapping>
                <layout type=""log4net.Layout.PatternLayout"">
                    <conversionPattern value=""%-5level [%date{HHmm.ss,ffff}][%thread] - %message - [%date{MM-dd-yyyy} %logger:%line]%newline""/>
                </layout>
            </appender>

            <!--Set root logger level to TRACE and its only appender to the rolling file appender -->
            <root>
                <level value=""TRACE""/>
                <appender-ref ref=""RollingFileAppender""/>
                <appender-ref ref=""ColoredConsoleAppender""/>
            </root>
        </log4net>");

        /// <summary>
        /// Get the precongigured log4net configuration DebugLoggerConfig as an XmlElement (which is required for XmlConfigurator.Configure)
        /// </summary>
        /// <returns></returns>
        public static XmlElement DebugLoggerConfigElement
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                return (XmlElement)doc.ReadNode(DebugLoggerConfig.CreateReader());
            }
        }

        /// <summary>
        /// Sets up the log4net system to use the Debug logger (a FileAppender with all messages logged) to a file with the name "logName"_log.txt
        /// </summary>
        /// <param name="logName"></param>
        public static void SetupDebugLogger(string logName)
        {
            log4net.GlobalContext.Properties["LogName"] = logName;
            //Me.GetType().FullName
            log4net.Config.XmlConfigurator.Configure(DebugLoggerConfigElement);
        }

        /// <summary>
        /// Call this function to set ALL loggers to use log4net as the logging system.
        /// </summary>
        public static void SetLog4NetAsDefaultLogger()
        {
            LogServices.Initialize(x => new Log4NetWrapper(), true);
        }

        //=======================================================
        //Service provided by Telerik (www.telerik.com)
        //Conversion powered by NRefactory.
        //Twitter: @telerik
        //Facebook: facebook.com/telerik
        //=======================================================
    }
}

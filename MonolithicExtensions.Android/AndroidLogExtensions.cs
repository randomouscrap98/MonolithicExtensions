using Android.Util;
using MonolithicExtensions.Portable.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonolithicExtensions.Android
{
    public static class AndroidLoggerExtensions
    {
        public static void SetAndroidAsDefaultLogger()
        {
            LogServices.Initialize(x => new AndroidLoggerWrapper(), true);
        }
    }

    public class AndroidLoggerWrapper : ILogger
    {
        public IList<ILogHandler> Handlers
        {
            get { return null; }
            set { /* do nothing */ }
        }

        public string Name { get; set; }

        public void Initialize(string name)
        {
            Name = name;
        }

        public void Trace(string message) { Log.Verbose(Name, message); }
        public void Debug(string message) { Log.Debug(Name, message); }
        public void Info(string message) { Log.Info(Name, message); }
        public void Warn(string message) { Log.Warn(Name, message); }
        public void Error(string message) { Log.Error(Name, message); }
        public void Fatal(string message) { Log.Wtf(Name, message); } //Note: this may be too severe!

        public void LogRaw(string message, int level)
        {
            try
            {
                switch((LogLevel)level)
                {
                    case LogLevel.Debug:
                        Debug(message);
                        break;
                    case LogLevel.Error:
                        Error(message);
                        break;
                    case LogLevel.Fatal:
                        Fatal(message);
                        break;
                    case LogLevel.Info:
                        Info(message);
                        break;
                    case LogLevel.Trace:
                        Trace(message);
                        break;
                    case LogLevel.Warn:
                        Warn(message);
                        break;
                    default:
                        Info(message);
                        break;
                }
            }
            catch(Exception ex)
            {
                //Potential for infinite recursion IF LogLevel.Error can't be converted to log4net
                Error($"Could not convert level {level} to Android level: {ex}");
            }
        }
    }
}

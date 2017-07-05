using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonolithicExtensions.Windows
{
    public class TextBoxAppender : AppenderSkeleton
    {
        public string TextBoxName { get; set; } = "";
        public int MaxBacklog { get; set; } = 100;
        private Queue<string> Backlog { get; set; } = new Queue<string>();

        public TextBoxAppender() { }

        protected override void Append(LoggingEvent loggingEvent)
        {
            Backlog.Enqueue(RenderLoggingEvent(loggingEvent));

            if (Backlog.Count > Math.Max(MaxBacklog, 1))
                Backlog.Dequeue();

            if (string.IsNullOrWhiteSpace(TextBoxName))
                return;

            foreach (Control control in FormControlExtensions.FindAllControlsByName(TextBoxName))
            {
                var textbox = control as TextBox;
                if (string.IsNullOrWhiteSpace(textbox.Text))
                {
                    textbox.SafeAppend(string.Join("", Backlog));
                    //textbox.BeginInvoke(Sub() textbox.AppendText(String.Join("", Backlog)))
                }
                else
                {
                    textbox.SafeAppend(Backlog.Last());
                    //textbox.BeginInvoke(Sub() textbox.AppendText(Backlog.Last()))
                }
            }
        }

        protected override bool RequiresLayout
        {
            get { return true; }
        }
    }

    public class Log4NetTextWriter : TextWriter
    {

        private string BufferedMessage { get; set; } = "";
        private log4net.ILog Logger { get; }
        public log4net.Core.Level DesiredLogLevel { get; set; } = Level.Info;

        public Log4NetTextWriter(log4net.ILog baseLogger)
        {
            Logger = baseLogger;
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void Write(char c)
        {
            BufferedMessage += c;

            if (c == '\n')
            {
                Logger.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, DesiredLogLevel, BufferedMessage, null);
                BufferedMessage = "";
            }
        }
    }
}

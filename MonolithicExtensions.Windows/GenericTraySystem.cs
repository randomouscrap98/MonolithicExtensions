using MonolithicExtensions.Portable.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonolithicExtensions.Windows
{
    public class GenericTrayItem : IDisposable
    {
        private bool disposed = false;
        private Form displayedForm = null;

        protected NotifyIcon TrayItem;
        protected ILogger Logger;

        public Func<object, Form> TrayFormCreator = null;
        public Action<Form, object> TrayFormCleanup = null;
        public object StateObject = null;

        public GenericTrayItem()
        {
            Logger = LogServices.CreateLoggerFromDefault(GetType());
            TrayItem = new NotifyIcon();
            TrayItem.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    ShowForm();
            };
            TrayItem.BalloonTipClicked += (s, e) =>
            {
                ShowForm();
            };
        }

        public void Show()
        {
            TrayItem.Visible = true;
        }

        public void Hide()
        {
            TrayItem.Visible = false;
        }

        /// <summary>
        /// Update the title and icon. Both are optional: if you simply want to update the icon, pass null for the title.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="icon"></param>
        public void Update(string title = null, System.Drawing.Icon icon = null)
        {
            if(icon != null) TrayItem.Icon = icon;
            if(title != null) TrayItem.Text = title;
        }

        public void ShowBalloon(string text, TimeSpan time, ToolTipIcon icon = ToolTipIcon.None)
        {
            TrayItem.BalloonTipIcon = icon;
            TrayItem.BalloonTipText = text;
            TrayItem.ShowBalloonTip((int)time.TotalMilliseconds);
        }

        private void ShowForm()
        {
            if(TrayFormCreator != null)
            {
                if(displayedForm == null) 
                {
                    try
                    {
                        displayedForm = TrayFormCreator(StateObject);
                        displayedForm.ShowDialog(); //It SHOULD get stuck here! That way, it can't release the lock until the form gets closed!
                        TrayFormCleanup?.Invoke(displayedForm, StateObject);
                    }
                    finally
                    {
                        displayedForm = null;
                    }
                }
                else
                {
                    //Form may have become null in the time it took to get down here. use ? for safety.
                    displayedForm?.Activate();
                }
            }
            else
            {
                Logger.Error($"Tried showing a form when no form creator was set!");
            }
        }

        public void SetMenu(Dictionary<string, Action> options)
        {
            if (TrayItem.ContextMenuStrip != null)
                TrayItem.ContextMenuStrip.Dispose();

            var menu = new ContextMenuStrip();

            foreach(var option in options)
            {
                var item = new ToolStripMenuItem();
                item.Text = option.Key;
                item.Click += (s, e) => option.Value();
                menu.Items.Add(item);
            }

            TrayItem.ContextMenuStrip = menu;
        }

        public void Dispose()
        {
            if(!disposed)
            {
                TrayItem.Dispose();
            }
            disposed = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;
using MonolithicExtensions.Portable.Logging;

namespace MonolithicExtensions.Android
{
    /// <summary>
    /// Create a "dialog" which can be shown later or in a different context, but the creator can still retrieve
    /// the result. Useful for services which want to pop dialogs only when an activity is on screen.
    /// </summary>
    public class PendingDialog
    {
        public int ID;
        public string Title;
        public string Message;
        public string PositiveText;
        public string NegativeText;
        public bool Cancellable;

        public Action<Context> PositiveAction;
        public Action<Context> NegativeAction;

        private int waiting;
        private int pending;
        private AutoResetEvent signal = new AutoResetEvent(false);
        private PendingDialogResult result;

        protected ILogger Logger;

        public PendingDialog()
        {
            Logger = LogServices.CreateLoggerFromDefault(this.GetType());
            Reset();
        }
        
        /// <summary>
        /// Fully reset the dialog and free any waiting threads
        /// </summary>
        public void Reset()
        {
            waiting = 0;
            pending = 0;
            result = PendingDialogResult.None;
            signal.Set(); //Cancel any pending stuff

            ID = 0;
            Title = "";
            Message = "";
            PositiveText = "";
            NegativeText = "";
            PositiveAction = null;
            NegativeAction = null;
            Cancellable = true;
        }

        /// <summary>
        /// You can set all these fields individually, or you can set them all at the same time with this function
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="positiveText"></param>
        /// <param name="negativeText"></param>
        /// <param name="cancellable"></param>
        public void QuickSetup(string title, string message, string positiveText, string negativeText, bool? cancellable = null)
        {
            this.Title = title;
            this.Message = message;
            this.PositiveText = positiveText;
            this.NegativeText = negativeText;
            if (cancellable != null) this.Cancellable = (bool)cancellable;
        }

        /// <summary>
        /// Wait for the dialog to complete and get the result.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <remarks>
        /// Only one thing can wait for the dialog result. Erm will that be a problem? 
        /// </remarks>
        public PendingDialogResult WaitForDialog(TimeSpan? timeout = null)
        {
            Logger.Trace("WaitForDialog called");

            signal.Reset();
            result = PendingDialogResult.None;
            Interlocked.Increment(ref waiting);
            Interlocked.Increment(ref pending);

            try
            {
                if (timeout != null)
                    signal.WaitOne((TimeSpan)timeout);
                else
                    signal.WaitOne();
            }
            finally
            {
                Interlocked.Exchange(ref waiting, 0);
            }

            Logger.Trace($"WaitForDialog complete! Result: {result}");
            return result;
        }

        /// <summary>
        /// Is this instance waiting for a dialog result?
        /// </summary>
        public bool IsWaiting
        {
            get { return waiting != 0; }
        }

        /// <summary>
        /// Is this instance waiting for the dialog to be shown?
        /// </summary>
        public bool IsPending
        {
            get { return pending != 0; }
        }

        /// <summary>
        /// Show the pending dialog in the given context. When the dialog is complete, it will signal 
        /// anyone waiting on this pending dialog
        /// </summary>
        /// <param name="context"></param>
        public void ShowDialog(Context context)
        {
            Logger.Trace("ShowDialog called");
            Interlocked.Exchange(ref pending, 0);
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle(Title).SetMessage(Message);
            builder.SetPositiveButton(PositiveText, (d, i) =>
            {
                result = PendingDialogResult.Positive;
                if (PositiveAction != null) PositiveAction.Invoke(context);
                signal.Set();
            });
            if (!String.IsNullOrWhiteSpace(NegativeText) || NegativeAction != null)
            {
                builder.SetNegativeButton(NegativeText, (d, i) =>
                {
                    result = PendingDialogResult.Negative;
                    if (NegativeAction != null) NegativeAction.Invoke(context);
                    signal.Set();
                });
            }
            builder.SetCancelable(Cancellable);
            builder.Create().Show();
        }
    }

    public enum PendingDialogResult
    {
        None,
        Positive,
        Negative
    }
}
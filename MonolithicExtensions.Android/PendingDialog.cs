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

        public void QuickSetup(string title, string message, string positiveText, string negativeText, bool? cancellable = null)
        {
            this.Title = title;
            this.Message = message;
            this.PositiveText = positiveText;
            this.NegativeText = negativeText;
            if (cancellable != null) this.Cancellable = (bool)cancellable;
        }

        //Only one thing can wait for the dialog result. Erm will that be a problem? 
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

        public bool IsWaiting
        {
            get { return waiting != 0; }
        }

        public bool IsPending
        {
            get { return pending != 0; }
        }

        public void ShowDialog(Context context)//, Action positiveAction = null, Action negativeAction = null)
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
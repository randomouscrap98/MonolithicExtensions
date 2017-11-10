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
        public int ID = 0;
        public string Title = "";
        public string Message = "";
        public string PositiveText = "";
        public string NegativeText = "";
        public bool Cancellable = true;

        private int waiting = 0;
        private int pending = 0;
        private AutoResetEvent signal = new AutoResetEvent(false);
        private PendingDialogResult result = PendingDialogResult.None;

        protected ILogger Logger;

        public PendingDialog()
        {
            Logger = LogServices.CreateLoggerFromDefault(this.GetType());
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

        public void ShowDialog(Context context, Action positiveAction = null, Action negativeAction = null)
        {
            Logger.Trace("ShowDialog called");
            Interlocked.Exchange(ref pending, 0);
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle(Title).SetMessage(Message);
            builder.SetPositiveButton(PositiveText, (d, i) =>
            {
                result = PendingDialogResult.Positive;
                if (positiveAction != null) positiveAction.Invoke();
                signal.Set();
            });
            builder.SetNegativeButton(NegativeText, (d, i) =>
            {
                result = PendingDialogResult.Negative;
                if (negativeAction != null) negativeAction.Invoke();
                signal.Set();
            });
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
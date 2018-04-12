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
using MonolithicExtensions.Portable.Logging;

namespace MonolithicExtensions.Android
{
    /// <summary>
    /// Simplifies some common Activity use cases; extend from this type of Activity to make life easier
    /// </summary>
    public class SimpleActivity : Activity
    {
        protected ILogger Logger;

        public SimpleActivity()
        {
            Logger = LogServices.CreateLoggerFromDefault(this.GetType());
        }

        /// <summary>
        /// Pop a toast on the UI thread. I don't like the Toast.MakeText.Show() pattern, so this is a single function that does everything
        /// </summary>
        /// <param name="message"></param>
        /// <param name="length"></param>
        public void DoToast(string message, ToastLength length = ToastLength.Short)
        {
            RunOnUiThread(() => Toast.MakeText(this, message, length).Show());
        }

        /// <summary>
        /// Pop a toast on the UI thread. I don't like the Toast.MakeText.Show() pattern, so this is a single function that does everything
        /// </summary>
        /// <param name="messageResource"></param>
        /// <param name="length"></param>
        public void DoToast(int messageResource, ToastLength length = ToastLength.Short)
        {
            RunOnUiThread(() => Toast.MakeText(this, messageResource, length).Show());
        }
    }
}
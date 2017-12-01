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
    public class SimpleActivity : Activity
    {
        protected ILogger Logger;

        public SimpleActivity()
        {
            Logger = LogServices.CreateLoggerFromDefault(this.GetType());
        }

        public void DoToast(string message, ToastLength length = ToastLength.Short)
        {
            RunOnUiThread(() => Toast.MakeText(this, message, length).Show());
        }

        public void DoToast(int messageResource, ToastLength length = ToastLength.Short)
        {
            RunOnUiThread(() => Toast.MakeText(this, messageResource, length).Show());
        }
    }
}
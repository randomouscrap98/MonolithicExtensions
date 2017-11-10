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
using Newtonsoft.Json;
using Android.Preferences;
using Android.Util;
using Java.Lang;

namespace MonolithicExtensions.Android
{
    public static class PreferenceExtensions
    {
        public static T GetSerialized<T>(this ISharedPreferences preferences, string name, string defaultValue)
        {
            return JsonConvert.DeserializeObject<T>(preferences.GetString(name, defaultValue));
        }

        public static void SetSerialized<T>(this ISharedPreferences preferences, string name, T data)
        {
            var editor = preferences.Edit();
            editor.PutString(name, JsonConvert.SerializeObject(data));
            editor.Commit();
        }
    }

    public class EditTextPreferenceShown : EditTextPreference
    {
        public EditTextPreferenceShown(Context context, IAttributeSet attributes, int defStyle) : base(context, attributes, defStyle) { }
        public EditTextPreferenceShown(Context context, IAttributeSet attributes) : base(context, attributes) { }
        public EditTextPreferenceShown(Context context) : base(context) { }

        public override ICharSequence SummaryFormatted
        {
            get { return new Java.Lang.String(Text); }//return new Java.Lang.String(string.Format(Summary, Text)); }
            set { base.SummaryFormatted = value; }
        }
    }
}
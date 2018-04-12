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
    /// <summary>
    /// Extension functions for android's SharedPreferences system
    /// </summary>
    public static class PreferenceExtensions
    {
        /// <summary>
        /// Retrieve a serialized setting from preferences as the given object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="preferences"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetSerialized<T>(this ISharedPreferences preferences, string name, string defaultValue)
        {
            return JsonConvert.DeserializeObject<T>(preferences.GetString(name, defaultValue));
        }

        /// <summary>
        /// Store an object as a string within SharedPreferences
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="preferences"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public static void SetSerialized<T>(this ISharedPreferences preferences, string name, T data)
        {
            var editor = preferences.Edit();
            editor.PutString(name, JsonConvert.SerializeObject(data));
            editor.Commit();
        }
    }

    /// <summary>
    /// An EditTextPreference that shows the value of the preference in the view (the default does not for some reason)
    /// </summary>
    public class EditTextPreferenceShown : EditTextPreference
    {
        public EditTextPreferenceShown(Context context, IAttributeSet attributes, int defStyle) : base(context, attributes, defStyle) { }
        public EditTextPreferenceShown(Context context, IAttributeSet attributes) : base(context, attributes) { }
        public EditTextPreferenceShown(Context context) : base(context) { }

        public override ICharSequence SummaryFormatted
        {
            get { return new Java.Lang.String(Text); }
            set { base.SummaryFormatted = value; }
        }
    }
}
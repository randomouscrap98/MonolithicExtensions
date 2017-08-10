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

namespace MonolithicExtensions.Android
{
    public static class ViewExtensions
    {
        public static List<T> FindAllViewsByType<T>(this ViewGroup container)
        {
            return container.FindAllViews(x => x is T).Cast<T>().ToList();
        }

        public static List<View> FindAllViewsByTag(this ViewGroup container, object tag)
        {
            return container.FindAllViews(x => x.Tag.Equals(tag));
        }

        public static List<View> FindAllViews(this ViewGroup container, Func<View, bool> filter)
        {
            List<View> results = new List<View>();

            for(int i = 0; i < container.ChildCount; i++)
            {
                var child = container.GetChildAt(i);
                if (child is ViewGroup)
                    results.AddRange(((ViewGroup)child).FindAllViews(filter));
                else if (filter(child))
                    results.Add(child);
            }

            return results;
        }
    }
}
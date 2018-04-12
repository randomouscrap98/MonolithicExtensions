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
    /// <summary>
    /// Extension functions for views and viewgroups
    /// </summary>
    public static class ViewExtensions
    {
        /// <summary>
        /// Find ALL views that match the given type (at all levels)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static List<T> FindAllViewsByType<T>(this ViewGroup container)
        {
            return container.FindAllViews(x => x is T).Cast<T>().ToList();
        }

        /// <summary>
        /// Find all views where the tag equals the given tag (at all levels)
        /// </summary>
        /// <param name="container"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static List<View> FindAllViewsByTag(this ViewGroup container, object tag)
        {
            return container.FindAllViews(x => x.Tag.Equals(tag));
        }

        /// <summary>
        /// Find all views at all levels that match the given filter
        /// </summary>
        /// <param name="container"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
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
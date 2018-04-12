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
using Java.Lang;

namespace MonolithicExtensions.Android 
{
    public class AdapterTypeData
    {
        public int Layout = -1;
        public Action<View, object> FillView = null;
        public bool Enabled = true;
    }

    /// <summary>
    /// An adapter for displaying multiple types of items in the same list each with their own layout.
    /// </summary>
    /// <remarks>
    /// Android doesn't provide a SIMPLE way to display a list of mismatched items each with their own layout. For instance, I simply wanted
    /// a list with headers to separate sections, but APPARENTLY this is "outside the scope" of android. Whatever, this class allows you to do that.
    /// For instance, if you want to display a list of objects grouped underneath various headers (like the preferences activity), you could set 
    /// the adapter type for String to be a header layout and the adapter type for your objects to be a different one, then make a big list
    /// of everything (headers and items) and use this adapter.
    /// </remarks>
    public class MultiItemAdapter : BaseAdapter
    {
        public const int ErrorID = -1;

        protected Context Context = null;
        protected Dictionary<Type, AdapterTypeData> AdapterTypes = new Dictionary<Type, AdapterTypeData>();

        public List<object> Items = new List<object>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">The current Android context being used for this adapter</param>
        /// <param name="adapterTypes">The types you wish to display in the list and how to display them. This MUST include all types you wish to display from the outset.</param>
        public MultiItemAdapter(Context context, Dictionary<Type, AdapterTypeData> adapterTypes)
        {
            this.Context = context;
            this.AdapterTypes = adapterTypes;
        }

        /// <summary>
        /// Get all types supported by this MultiItemAdapter (that you passed in)
        /// </summary>
        public List<Type> AdapterItemTypes
        {
            get { return AdapterTypes.Keys.OrderBy(x => x.Name).ToList();  }
        }

        /// <summary>
        /// Get the data used to display the type for the item in the given position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public AdapterTypeData GetTypeOfItem(int position)
        {
            var type = GetItemViewType(position);
            if (type == ErrorID) return null;
            return AdapterTypes[AdapterItemTypes[type]];
        }

        public override int Count
        {
            get { return Items.Count; }
        }

        public override int ViewTypeCount
        {
            get { return AdapterItemTypes.Count; }
        }

        public override int GetItemViewType(int position)
        {
            if (position >= Items.Count) return ErrorID;

            var types = AdapterItemTypes;
            for(int i = 0; i < types.Count; i++)
            {
                if (types[i].IsInstanceOfType(Items[position]))
                    return i;
            }

            return ErrorID;
        }

        /// <summary>
        /// I didn't want to construct Java objects from .NET objects, so this returns null. Nothing really needs this anyway since Items is exposed.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        /// <summary>
        /// Get whether or not a particular item is enabled. This is useful for disabling the click listener for header sections/etc.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override bool IsEnabled(int position)
        {
            var type = GetTypeOfItem(position);
            return type != null && type.Enabled;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var type = GetTypeOfItem(position);

            if(type != null)
            {
                if(convertView == null)
                    convertView = LayoutInflater.From(Context).Inflate(type.Layout, parent, false);

                type.FillView(convertView, Items[position]);
            }

            return convertView;
        }
    }
}
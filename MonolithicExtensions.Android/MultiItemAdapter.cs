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
    //public class SectionHeader
    //{
    //    public string Title = "";
    //}

    public class AdapterTypeData
    {
        public int Layout = -1;
        public Action<View, object> FillView = null;
        public bool Enabled = true;
    }

    public class MultiItemAdapter : BaseAdapter
    {
        public const int ErrorID = -1;

        protected Context Context = null;
        protected Dictionary<Type, AdapterTypeData> AdapterTypes = new Dictionary<Type, AdapterTypeData>();

        public object[] Items = new object[0];

        public MultiItemAdapter(Context context, Dictionary<Type, AdapterTypeData> adapterTypes)
        {
            this.Context = context;
            this.AdapterTypes = adapterTypes;
        }

        public List<Type> AdapterItemTypes
        {
            get { return AdapterTypes.Keys.OrderBy(x => x.Name).ToList();  }
        }

        public AdapterTypeData GetTypeOfItem(int position)
        {
            var type = GetItemViewType(position);
            if (type == ErrorID) return null;
            return AdapterTypes[AdapterItemTypes[type]];
        }


        public override int Count
        {
            get { return Items.Length; }
        }

        public override int ViewTypeCount
        {
            get { return AdapterItemTypes.Count; }
        }

        public override int GetItemViewType(int position)
        {
            if (position >= Items.Length) return ErrorID;

            var types = AdapterItemTypes;
            for(int i = 0; i < types.Count; i++)
            {
                if (types[i].IsInstanceOfType(Items[position]))
                    return i;
            }

            return ErrorID;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            //Apparently we can simply return null.
            return null;
            //if (position >= Items.Length) return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

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
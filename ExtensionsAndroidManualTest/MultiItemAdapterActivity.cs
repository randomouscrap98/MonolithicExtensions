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
using MonolithicExtensions.Android;
using static Android.Widget.AdapterView;

namespace ExtensionsAndroidManualTest
{
    [Activity(Label = "MultiItemAdapter")]
    public class MultiItemAdapterActivity : SimpleActivity
    {
        public const int ItemCount = 100;
        public const int ItemsPerTitle = 10;

        ListView multiItemListView;
        MultiItemAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView (Resource.Layout.MultiItemAdapterTest);
            multiItemListView = FindViewById<ListView>(Resource.Id.multiItemListView);

            adapter = new MultiItemAdapter(this, new Dictionary<Type, AdapterTypeData>()
            {
                { typeof(Guid), new AdapterTypeData()
                    {
                        Enabled = true,
                        Layout = Resource.Layout.MultiItemGuid,
                        FillView = (v, o) =>
                        {
                            var text = v.FindViewById<TextView>(Resource.Id.guidText);
                            text.Text = o.ToString();
                        }
                    }
                },
                { typeof(string), new AdapterTypeData()
                    {
                        Enabled = false,
                        Layout = Resource.Layout.MultiItemTitle,
                        FillView = (v, o) =>
                        {
                            var text = v.FindViewById<TextView>(Resource.Id.titleText);
                            text.Text = o.ToString();
                        }
                    }
                }
            });

            //adapter.Items = new object[ItemCount];
            for(int i = 0; i < ItemCount; i++)
            {
                if(i % ItemsPerTitle == 0)
                    adapter.Items.Add($"Section {i/ItemsPerTitle + 1} ☢");
                else
                    adapter.Items.Add(Guid.NewGuid());
            }

            multiItemListView.Adapter = adapter;
            multiItemListView.ItemClick += (s, e) =>
            {
                DoToast($"You clicked: {adapter.Items[e.Position]}");
            };
        }
    }
}
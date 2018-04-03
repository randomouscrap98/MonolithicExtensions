using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;

namespace ExtensionsAndroidManualTest
{
    [Activity(Label = "ExtensionsAndroidManualTest", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        Button testMultiItemAdapterButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            testMultiItemAdapterButton = FindViewById<Button>(Resource.Id.multiItemTestButton);
            testMultiItemAdapterButton.Click += TestMultiItemAdapterButton_Click;
        }

        private void TestMultiItemAdapterButton_Click(object sender, System.EventArgs e)
        {
            var testIntent = new Intent(this, typeof(MultiItemAdapterActivity));
            StartActivity(testIntent);
        }
    }
}


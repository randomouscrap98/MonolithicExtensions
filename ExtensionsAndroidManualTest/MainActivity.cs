using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;

/// <remarks>
/// THIS PROJECT IS UNNECESSARY!
/// This project/namespace exists to perform manual (usually user-input related) tests. Although these can and should be automated,
/// I was in a rush and made this quickly. This project is unnecessary: should you want to clean up this solution, you can safely remove
/// this project (unless you really want the 1 test that's in here)
/// </remarks>
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


using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace MystifierLight
{
    [Activity(Label = "MystifierLight", Icon = "@drawable/icon")]
    public class IntroActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Intro);

            var btnOpenEditor = FindViewById<Button>(Resource.Id.btnLaunchEditor);
            btnOpenEditor.Click += (sender, args) => StartActivity(new Intent(Application.Context, typeof(EditorActivity)));

            var btnViewDocs = FindViewById<Button>(Resource.Id.btnViewDocs);
            btnViewDocs.Click += (sender, args) =>
            {
                var uri = Android.Net.Uri.Parse("https://iridiumion.xyz/projects/mystifier/myslight-help.html");
                var launchBrowserInt = new Intent(Intent.ActionView, uri);
                StartActivity(launchBrowserInt);
            };
        }
    }
}
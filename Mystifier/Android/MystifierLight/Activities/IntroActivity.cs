using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace MystifierLight.Activities
{
    [Activity(Label = "MystifierLight", Icon = "@drawable/icon")]
    public class IntroActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Intro);

            string startFile = null;
            var incomingIntent = Intent;
            if (incomingIntent != null)
            {
                Android.Net.Uri inputFileUri = incomingIntent.Data;
                if (inputFileUri != null)
                {
                    try
                    {
                        startFile = ExtractFileFromUri(inputFileUri);
                    }
                    catch (Java.Lang.Exception ex)
                    {
                        Toast.MakeText(this, "Error:\n" + ex.Message, ToastLength.Long).Show();
                        return;
                    }
                    if (startFile == null)
                    {
                        return;
                    }
                }
            }

            if (startFile != null)
            {
                var edLauncher = new Intent(Application.Context, typeof(EditorActivity));
                edLauncher.PutExtra("preloadFile", startFile);
                StartActivity(edLauncher);
            }

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

        private string ExtractFileFromUri(Android.Net.Uri inputFileUri)
        {
            string fileName = null;
            if (inputFileUri.Scheme == "file")
            {
                fileName = inputFileUri.LastPathSegment;
            }
            return fileName;
        }
    }
}
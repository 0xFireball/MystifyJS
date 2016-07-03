using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;

namespace MystifierLight
{
    [Activity(Label = "MystifierLight", Icon = "@drawable/icon")]
    public class IntroActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Intro);

            var btnOpenEditor = FindViewById<Button>(Resource.Id.btnLaunchEditor);
            btnOpenEditor.Click += (sender, args) => StartActivity(new Intent(Application.Context, typeof(EditorActivity)));
        }
    }
}


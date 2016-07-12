using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;

namespace MystifierLight.Activities
{
    [Activity(Label = "MystifierLight", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/MystifyJS.Splash")]
    public class SplashScreenActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SplashScreen);

            var startupWork = new Task(() =>
            {
                //TODO: Initialize here
                Task.Delay(400);
            });

            startupWork.ContinueWith(t =>
            {
                //Ready to start application:
                StartActivity(new Intent(Application.Context, typeof(IntroActivity)));
            }, TaskScheduler.FromCurrentSynchronizationContext());

            startupWork.Start();
        }
    }
}
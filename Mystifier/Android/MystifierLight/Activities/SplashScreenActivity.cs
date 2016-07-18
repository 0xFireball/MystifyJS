using Android.App;
using Android.Content;
using Android.OS;
using JSONPush;
using System.Threading.Tasks;

namespace MystifierLight.Activities
{
    [Activity(Label = "MystifierLight", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/MystifyJS.Splash")]
    public class SplashScreenActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SplashScreen);

            var startupWork = new Task(async () =>
            {
                //TODO: Initialize here
                var pushClient = new JsonPushClient("https://push.iridiumion.xyz/myslight/push.json");
                var feedFetchStatus = await pushClient.FetchFeed();
                if (feedFetchStatus)
                {
                    pushClient.DisplayPendingMessages(this);
                }
                await Task.Delay(100);
            });

            startupWork.ContinueWith(t =>
            {
                //Ready to start application:
                var introIntent = new Intent(Application.Context, typeof(IntroActivity));
                StartActivity(introIntent);
            }, TaskScheduler.FromCurrentSynchronizationContext());

            startupWork.Start();
        }
    }
}
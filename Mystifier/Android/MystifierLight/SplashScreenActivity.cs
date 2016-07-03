using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MystifierLight
{
    [Activity(Label = "MystifierLight", MainLauncher = true, Icon = "@drawable/icon", Theme= "@style/MystifyJS.Splash")]
    public class SplashScreenActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SplashScreen);

            var startupWork = new Task(() => {
                //TODO: Initialize here
            });

            startupWork.ContinueWith(t => {
                //Ready to start application:
                StartActivity(new Intent(Application.Context, typeof(IntroActivity)));
            }, TaskScheduler.FromCurrentSynchronizationContext());

            startupWork.Start();
        }
    }
}
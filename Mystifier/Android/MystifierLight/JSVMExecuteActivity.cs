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
using Jint;
using Mystifier.DarkMagic.JSVM;

namespace MystifierLight
{
    [Activity(Label = "JSVMExecuteActivity")]
    public class JSVMExecuteActivity : Activity
    {
        private string _codeToExecute;
        private TextView outputTv;
        private Button returnBtn;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.JExecute);
            _codeToExecute = Intent.GetStringExtra("code") ?? "";
            InitializeComponent();

            WireEvents();

            ExecuteLoadedCode();
        }

        private void ExecuteLoadedCode()
        {
            var jsEngine = new Engine(cfg => { cfg.AllowClr(ExaJSInit.GetExaJSAssemblies()); });
            
        }

        private void WireEvents()
        {
            returnBtn.Click += (sender, args) =>
            {
                OnBackPressed();
            };
        }

        private void InitializeComponent()
        {
            outputTv = FindViewById<TextView>(Resource.Id.outputTxView);
            returnBtn = FindViewById<Button>(Resource.Id.btnBackToEditor);
        }
    }
}
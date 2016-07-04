using System;
using Android.App;
using Android.OS;
using Android.Widget;

namespace MystifierLight
{
    [Activity(Label = "EditorActivity", Icon = "@drawable/icon")]
    public class EditorActivity : Activity
    {
        private EditText jsEditor;
        private Button btnExecute;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Editor);

            InitializeComponent();
            WireEvents();
        }

        private void WireEvents()
        {
            btnExecute.Click += BtnExecuteOnClick;
        }

        private void BtnExecuteOnClick(object sender, EventArgs eventArgs)
        {
            //Execute code
            AlertDialog.Builder msg = new AlertDialog.Builder(this);
            msg.SetTitle("Coming soon");
            msg.SetMessage("This feature has not yet been implemented. Thank you for using Mystifier.");
            msg.Show();
        }

        private void InitializeComponent()
        {
            jsEditor = FindViewById<EditText>(Resource.Id.jsEditor);
            btnExecute = FindViewById<Button>(Resource.Id.btnExecute);
        }
    }
}
using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Mystifier.DarkMagic.EditorUtils;

namespace MystifierLight
{
    [Activity(Label = "Editor", Icon = "@drawable/icon")]
    public class EditorActivity : Activity
    {
        private EditText jsEditor;
        private Button btnExecute;
        private Button btnBeautify;
        private bool _isUnsaved;
        private Button btnTools;

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
            btnBeautify.Click += BtnBeautifyOnClick;
            btnTools.Click += BtnToolsOnClick;
        }

        private void BtnToolsOnClick(object sender, EventArgs eventArgs)
        {
            var popup = new PopupMenu(this, btnTools);
            popup.MenuInflater.Inflate(Resource.Menu.EditorToolsMenu, popup.Menu);
            //Wire popup events
            popup.Show();
        }

        private async void BtnBeautifyOnClick(object sender, EventArgs eventArgs)
        {
            var editorSource = jsEditor.Text;
            _isUnsaved = true;
            var beautifiedSource = editorSource;
            await Task.Run(() =>
            {
                var beautifier = Beautifier.CreateDefault();
                beautifiedSource = beautifier.Beautify(editorSource);
            });
            jsEditor.Text = beautifiedSource;
        }

        private void BtnExecuteOnClick(object sender, EventArgs eventArgs)
        {
            //Execute code
            var msg = new AlertDialog.Builder(this);
            /* Not Implemented
            msg.SetTitle("Coming soon");
            msg.SetMessage("This feature has not yet been implemented. Thank you for using Mystifier.");
            msg.Show();
            */
            var executeIntent = new Intent(this, typeof(JSVMExecuteActivity));
            executeIntent.PutExtra("code", jsEditor.Text);
            StartActivity(executeIntent);
        }

        private void InitializeComponent()
        {
            jsEditor = FindViewById<EditText>(Resource.Id.jsEditor);
            btnExecute = FindViewById<Button>(Resource.Id.btnExecute);
            btnBeautify = FindViewById<Button>(Resource.Id.btnBeautify);
            btnTools = FindViewById<Button>(Resource.Id.btnTools);
        }
    }
}
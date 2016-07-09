using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using ExaPhaser.FilePicker;
using Mystifier.DarkMagic.EditorUtils;
using MystifierLight.Util;

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

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case FilePickerActivity.ResultCodeDirSelected:
                    switch (resultCode)
                    {
                        case Result.Canceled:
                            break;

                        case Result.FirstUser:
                            break;

                        case Result.Ok:
                            var selectedFile = data.GetStringExtra(FilePickerActivity.ResultSelectedDir);
                            var requestType = data.GetStringExtra(FilePickerActivity.RequestDescriptor);
                            switch (requestType)
                            {
                                case "openFile":
                                    var fileCnt = File.ReadAllText(selectedFile);
                                    jsEditor.Text = fileCnt;
                                    break;

                                case "saveFile":
                                    var editorCnt = jsEditor.Text;
                                    File.WriteAllText(selectedFile, editorCnt);
                                    break;
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(resultCode));
                    }
                    break;
            }
        }

        private void BtnToolsOnClick(object sender, EventArgs eventArgs)
        {
            var popup = new PopupMenu(this, btnTools);
            popup.MenuInflater.Inflate(Resource.Menu.EditorToolsMenu, popup.Menu);
            popup.MenuItemClick += (s, args) =>
            {
                var selectedItem = args.Item.TitleFormatted.ToString();
                var filePickerIntent = new Intent(this, typeof(FilePickerActivity));
                switch (selectedItem)
                {
                    case "Save File":
                        filePickerIntent.PutExtra(FilePickerActivity.ExtraMode, (int)FilePickerMode.File);
                        filePickerIntent.PutExtra(FilePickerActivity.RequestDescriptor, "saveFile");
                        filePickerIntent.PutExtra(FilePickerActivity.DialogTitle, "Save File");
                        StartActivityForResult(filePickerIntent, FilePickerActivity.ResultCodeDirSelected);
                        break;

                    case "Open File":
                        filePickerIntent.PutExtra(FilePickerActivity.ExtraMode, (int)FilePickerMode.File);

                        filePickerIntent.PutExtra(FilePickerActivity.RequestDescriptor, "openFile");
                        filePickerIntent.PutExtra(FilePickerActivity.DialogTitle, "Open File");
                        StartActivityForResult(filePickerIntent, FilePickerActivity.ResultCodeDirSelected);
                        break;

                    case "Open from URL":
                        DialogUtil.ShowInputDialog(this, "Enter URL", async (dlUrl) =>
                        {
                            try
                            {
                                if (dlUrl != null)
                                {
                                    var wc = new WebClient();
                                    Toast.MakeText(this, "Downloading...", ToastLength.Short);
                                    var dlContents = await wc.DownloadStringTaskAsync(dlUrl);
                                    jsEditor.Text = dlContents;
                                    Toast.MakeText(this, "Downloaded", ToastLength.Short);
                                }
                                else
                                {
                                    Toast.MakeText(this, "Canceled", ToastLength.Short);
                                }
                            }
                            catch (Exception ex)
                            {
                                Toast.MakeText(this, $"Error downloading: {ex.Message}", ToastLength.Short);
                            }
                        });

                        break;
                }
            };
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
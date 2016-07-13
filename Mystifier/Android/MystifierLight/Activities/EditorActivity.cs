using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using ExaPhaser.FilePicker;
using Java.Lang;
using Mystifier.DarkMagic.EditorUtils;
using Mystifier.DarkMagic.Obfuscators;
using MystifierLight.Fragments;
using MystifierLight.Util;
using MystifierLightEditor.Controls;
using MystifierLightEditor.SyntaxHighlighting;

namespace MystifierLight.Activities
{
    [Activity(Label = "Editor", Icon = "@drawable/icon")]
    public class EditorActivity : Activity, IOnTextChangedListener
    {
        private IridiumHighlightingEditor _jsEditor;
        private Button _btnExecute;
        private Button _btnBeautify;
        private bool _isUnsaved;
        private Button _btnTools;
        private EditorFragment _editorFragment;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Editor);

            InitializeComponent();
            WireEvents();
        }

        private void WireEvents()
        {
            _btnExecute.Click += BtnExecuteOnClick;
            _btnBeautify.Click += BtnBeautifyOnClick;
            _btnTools.Click += BtnToolsOnClick;
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
                                    _jsEditor.Text = fileCnt;
                                    break;

                                case "saveFile":
                                    var editorCnt = _jsEditor.Text;
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
            var popup = new PopupMenu(this, _btnTools);
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
                                    var dlToast = Toast.MakeText(this, "Downloading...", ToastLength.Short);
                                    dlToast.SetGravity(Android.Views.GravityFlags.Bottom | Android.Views.GravityFlags.CenterHorizontal, 0, getYOffset(this));
                                    dlToast.Show();
                                    var dlContents = await wc.DownloadStringTaskAsync(dlUrl);
                                    _jsEditor.Text = dlContents;
                                    var notifToast = Toast.MakeText(this, "Downloaded", ToastLength.Short);
                                    notifToast.SetGravity(Android.Views.GravityFlags.Bottom | Android.Views.GravityFlags.CenterHorizontal, 0, getYOffset(this));
                                    notifToast.Show();
                                }
                                else
                                {
                                    Toast.MakeText(this, "Canceled", ToastLength.Short);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Toast.MakeText(this, $"Error downloading: {ex.Message}", ToastLength.Short);
                            }
                        });

                        break;

                    case "Obfuscate Source":
                        //Coming soon!
                        break;
                }
            };
            //Wire popup events
            popup.Show();
        }

        private int getYOffset(Activity activity)
        {
            int yOffset = 0;
            if (yOffset == 0)
            {
                float dp = Resources.DisplayMetrics.Density;

                try
                {
                    ActionBar actionBar = this.ActionBar;

                    if (actionBar != null)
                        yOffset = actionBar.Height;
                }
                catch (ClassCastException e)
                {
                    yOffset = Java.Lang.Math.Round(48f * dp);
                }

                yOffset += Java.Lang.Math.Round(16f * dp);
            }

            return yOffset;
        }

        private async void BtnBeautifyOnClick(object sender, EventArgs eventArgs)
        {
            var editorSource = _jsEditor.Text;
            _isUnsaved = true;
            var beautifiedSource = editorSource;
            await Task.Run(() =>
            {
                var beautifier = Beautifier.CreateDefault();
                beautifiedSource = beautifier.Beautify(editorSource);
            });
            _jsEditor.Text = beautifiedSource;
        }

        private void BtnExecuteOnClick(object sender, EventArgs eventArgs)
        {
            //Execute code
            var msg = new AlertDialog.Builder(this);
            var executeIntent = new Intent(this, typeof(JsvmExecuteActivity));
            executeIntent.PutExtra("code", _jsEditor.Text);
            StartActivity(executeIntent);
        }

        private void InitializeComponent()
        {
            _editorFragment = FragmentManager.FindFragmentById<EditorFragment>(Resource.Id.jsEditorFragment);
            _jsEditor = FindViewById<IridiumHighlightingEditor>(Resource.Id.jsEditor);
            _btnExecute = FindViewById<Button>(Resource.Id.btnExecute);
            _btnBeautify = FindViewById<Button>(Resource.Id.btnBeautify);
            _btnTools = FindViewById<Button>(Resource.Id.btnTools);
        }

        public void OnTextChanged(string text)
        {
            //Handle text change
        }

        private static string ObfuscateJsSource(string inputSource)
        {
            var obfuscatedSource = inputSource;
            var obfuscators = new List<BaseObfuscator>
            {
                new RenamingScrambler(),
                new PackingScrambler(),
                new UnicodeEncodingScrambler()
            };
            foreach (var obfuscationEngine in obfuscators)
            {
                obfuscationEngine.LoadCode(obfuscatedSource);
                obfuscatedSource = obfuscationEngine.ObfuscateCode();
            }
            return obfuscatedSource;
        }
    }
}
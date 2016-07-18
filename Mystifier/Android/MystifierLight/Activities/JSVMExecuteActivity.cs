using Android.App;
using Android.OS;
using Android.Widget;
using IridiumJS;
using IridiumJS.Parser;
using IridiumJS.Runtime;
using Mystifier.DarkMagic.JSVM;
using MystifierLight.Util;
using System.Reflection;
using System.Threading.Tasks;

namespace MystifierLight.Activities
{
    [Activity(Label = "Execute JavaScript")]
    public class JsvmExecuteActivity : Activity
    {
        private string _codeToExecute;
        private TextView _outputTv;
        private Button _returnBtn;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.JExecute);
            _codeToExecute = Intent.GetStringExtra("code") ?? "";
            InitializeComponent();
            WireEvents();

            ExecuteLoadedCode();
        }

        private async void ExecuteLoadedCode()
        {
            var jsEngine = new IridiumJSEngine(cfg => { cfg.AllowClr(ExaJSInit.GetExaJSAssemblies()); });
            var console = new JSConsole(_outputTv, this);
            jsEngine.SetValue("console", console);
            try
            {
                jsEngine.Execute(_codeToExecute);
            }
            catch (JavaScriptException jEx)
            {
                await Task.Run(() => console.WriteLine($"{jEx.LineNumber},{jEx.Column} - {jEx.Error}"));
            }
            catch (ParserException pEx)
            {
                await Task.Run(() => console.WriteLine($"{pEx.LineNumber},{pEx.Column} - {pEx.Description}"));
            }
            catch (TargetInvocationException tEx)
            {
                await Task.Run(() => console.WriteLine(
                    $"{tEx.InnerException.GetType().Name} - {tEx.InnerException.Message}"));
            }
        }

        private void WireEvents()
        {
            _returnBtn.Click += (sender, args) =>
            {
                OnBackPressed();
            };
        }

        private void InitializeComponent()
        {
            _outputTv = FindViewById<TextView>(Resource.Id.outputTxView);
            _returnBtn = FindViewById<Button>(Resource.Id.btnBackToEditor);
        }
    }
}
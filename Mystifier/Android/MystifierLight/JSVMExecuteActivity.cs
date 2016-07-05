using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Jint;
using Jint.Parser;
using Jint.Runtime;
using Mystifier.DarkMagic.JSVM;
using MystifierLight.Util;

namespace MystifierLight
{
    [Activity(Label = "Execute JavaScript")]
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

        private async void ExecuteLoadedCode()
        {
            var jsEngine = new Engine(cfg => { cfg.AllowClr(ExaJSInit.GetExaJSAssemblies()); });
            var console = new JSConsole(outputTv, this);
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
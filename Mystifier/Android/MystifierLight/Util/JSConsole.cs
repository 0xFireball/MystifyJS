using Android.App;
using Android.Widget;

namespace MystifierLight.Util
{
    internal class JSConsole
    {
        public TextView OutputTextView { get; }

        public Activity ExecutionContext { get; set; }

        public JSConsole(TextView outputTextView, Activity executionContext)
        {
            OutputTextView = outputTextView;
            ExecutionContext = executionContext;
        }

        public void WriteLine(string str)
        {
            ExecutionContext.RunOnUiThread(() =>
            {
                OutputTextView.Text += str + "\n";
            });
        }

        // ReSharper disable once InconsistentNaming
        public void log(object obj)
        {
            WriteLine(obj.ToString());
        }

        // ReSharper disable once InconsistentNaming
        public void clear()
        {
            ExecutionContext.RunOnUiThread(() =>
            {
                OutputTextView.Text = "";
            });
        }
    }
}
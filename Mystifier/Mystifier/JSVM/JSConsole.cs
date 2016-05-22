using System.Windows.Controls;

namespace Mystifier.JSVM
{
    internal class JSConsole
    {
        private readonly TextBox _outputBox;

        public JSConsole(TextBox outputBox)
        {
            _outputBox = outputBox;
        }

        public void WriteLine(string format, params object[] args)
        {
            _outputBox.AppendText(string.Format(format, args) + "\n");
        }

        // ReSharper disable once InconsistentNaming
        public void log(object obj)
        {
            WriteLine(obj.ToString());
        }

        public void Clear()
        {
            _outputBox.Text = "";
        }
    }
}
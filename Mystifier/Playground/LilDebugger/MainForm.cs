using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using IridiumJS;
using IridiumJS.Runtime.Debugger;

namespace LilDebugger
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Debug the code
            var code = codeTb.Text;
            string[] codeLines = code.Split('\n').ToList().Select(str => str.TrimEnd()).ToArray();
            var engine = new IridiumJSEngine(options => { options.DebugMode(); });
            engine.Break += EngineStep;
            for (int i = 0; i< codeLines.Length; i++)
            {
                engine.BreakPoints.Add(new BreakPoint(i, 0));
            }
            engine.Execute(code);
        }

        private StepMode EngineStep(object sender, DebugInformation e)
        {
            return StepMode.Into;
        }
    }
}
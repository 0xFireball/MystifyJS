namespace Mystifier
{
    /// <summary>
    ///     Interaction logic for OnCrash.xaml
    /// </summary>
    public partial class OnCrash
    {
        public OnCrash(string crashTrace)
        {
            InitializeComponent();
            tbCrashInfo.Text = crashTrace;
        }
    }
}
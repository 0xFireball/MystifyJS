using System.Reflection;
using System.Windows;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;

namespace Mystifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadEditorTheme();
        }

        private void LoadEditorTheme()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mystifier.Resources.JavaScriptTheme.xshd"))
            {
                using (var reader = new XmlTextReader(stream))
                {
                    TextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    SearchPanel.Install(TextEditor);
                }
            }
        }
    }
}
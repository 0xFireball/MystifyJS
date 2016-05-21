using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using Mystifier.Activation;

namespace Mystifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MystifierActivation _activationProvider;

        public MainWindow()
        {
            InitializeComponent();
            _activationProvider = new MystifierActivation();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadEditorTheme();
            var isActivated = await Task.Run((Func<bool>)_activationProvider.CheckActivation);
            if (isActivated)
            {
                btnActivate.Visibility = Visibility.Hidden;
            }
            else
            {
                Title += " [Free Trial]";
            }
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

        private void BtnZetaPhase_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://zetaphase.io");
        }

        private void BtnActivate_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
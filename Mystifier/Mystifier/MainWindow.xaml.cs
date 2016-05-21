using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using Mystifier.Activation;
using Mystifier.IntelliJS.CodeCompletion;

namespace Mystifier
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MystifierActivation _activationProvider;
        private readonly bool _enableCodeCompletion;
        private CompletionWindow _completionWindow;
        private string _currentFile;
        private bool _isUnsaved;

        public MainWindow()
        {
            InitializeComponent();
            _activationProvider = new MystifierActivation();
            _enableCodeCompletion = false;
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadEditorTheme();
            var isActivated = await Task.Run((Func<bool>) _activationProvider.CheckActivation);
            if (isActivated)
            {
                btnActivate.Visibility = Visibility.Hidden;
            }
            else
            {
                Title += " [Free Trial]";
            }
            TextEditor.TextArea.TextEntering += TextAreaOnTextEntering;
            TextEditor.TextArea.TextEntered += TextAreaOnTextEntered;
            TextEditor.TextArea.KeyDown += TextAreaOnKeyDown;
            UpdateTitle();
        }

        private void TextAreaOnKeyDown(object sender, KeyEventArgs e)
        {
            //Keyboard shortcuts
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;
                if (e.Key == Key.S)
                {
                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    {
                        OnSaveFileAs();
                    }
                    else
                    {
                        OnSaveFile();
                    }
                }
                if (e.Key == Key.O)
                {
                    OnLoadFile();
                }
            }
        }

        private void OnLoadFile()
        {
            var newFile = MystifierUtil.BrowseForOpenFile("JavaScript Source Files (*.js)|*.js|All Files (*.*)|*.*",
                "Load File");
            if (newFile != null)
            {
                _currentFile = newFile;
                TextEditor.Load(_currentFile);
                UpdateTitle();
            }
        }

        private void OnSaveFileAs()
        {
            var previousFile = _currentFile;
            _currentFile = null;
            OnSaveFile();
            if (_currentFile == null)
            {
                _currentFile = previousFile;
            }
        }

        private void OnSaveFile()
        {
            if (_currentFile != null)
            {
                TextEditor.Save(_currentFile);
                _isUnsaved = false;
                UpdateTitle();
            }
            else
            {
                _currentFile = MystifierUtil.BrowseForSaveFile(
                    "JavaScript Source Files (*.js)|*.js|All Files (*.*)|*.*", "Save File");
                if (_currentFile == null)
                {
                    return;
                }
                OnSaveFile();
            }
        }

        private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "." && _enableCodeCompletion)
            {
                _completionWindow = new CompletionWindow(TextEditor.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;
                var codeCompletionProvider = new IntelligentJavaScriptCodeCompletionProvider();
                foreach (var completionData in codeCompletionProvider.GetCompletionOptions())
                {
                    data.Add(completionData);
                }
                _completionWindow.Show();
                _completionWindow.Closed += delegate { _completionWindow = null; };
            }
            //Auto-pair characters
            var editorCaret = TextEditor.TextArea.Caret;
            switch (e.Text)
            {
                case "{":
                    TextEditor.Document.Insert(editorCaret.Offset, "}");
                    editorCaret.Offset--; //Go back one char
                    break;

                case "(":
                    TextEditor.Document.Insert(editorCaret.Offset, ")");
                    editorCaret.Offset--; //Go back one char
                    break;

                case ")":
                    if (TextEditor.Document.GetCharAt(editorCaret.Offset - 2) == '(')
                    {
                        e.Handled = true; //Cancel the character
                        TextEditor.Document.Remove(editorCaret.Offset - 1, 1);
                    }
                    break;

                case "\"":
                    TextEditor.Document.Insert(editorCaret.Offset, "\"");
                    editorCaret.Offset--; //Go back one char
                    break;
            }
            _isUnsaved = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            if (_currentFile == null)
            {
                tbFileName.Text = _isUnsaved ? "[New File*]" : "[New File]";
            }
            else
            {
                var fn = Path.GetFileName(_currentFile);
                tbFileName.Text = _isUnsaved ? fn + "*" : fn;
            }
        }

        private void TextAreaOnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void LoadEditorTheme()
        {
            using (
                var stream =
                    Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("Mystifier.Resources.JavaScriptTheme.xshd"))
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
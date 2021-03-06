﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using IridiumJS;
using IridiumJS.Parser;
using IridiumJS.Runtime;
using MahApps.Metro.Controls.Dialogs;
using Mystifier.DarkMagic.EditorUtils;
using Mystifier.DarkMagic.JSVM;
using Mystifier.DarkMagic.Obfuscators;
using Mystifier.EditorTools;
using Mystifier.GitHub;
using Mystifier.IntelliJS.CodeCompletion;
using Mystifier.JSVM;
using Octokit;

namespace Mystifier
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Private Fields

        private readonly bool _enableCodeCompletion;
        private CompletionWindow _completionWindow;
        private string _currentFile;
        private string _currentGist;
        private string _currentGistId;
        private bool _editingGist;
        private bool _forceClose = false;
        private MystifierGitHubAccess _gitHubAccessProvider;
        private bool _isUnsaved;

        #endregion Private Fields

        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Application.Current.DispatcherUnhandledException +=
                (sender, args) =>
                {
                    args.Handled = true;
                    new OnCrash(args.Exception.ToString()) { Owner = this }.Show();
                };
            _gitHubAccessProvider = new MystifierGitHubAccess();
            _enableCodeCompletion = false;
        }

        #endregion Public Constructors

        #region Public Properties

        public string ProductUrl { get; set; } = "https://exaphaser.binpress.com/product/mystifier-studio/3826";

        #endregion Public Properties

        #region Private Methods

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

        private string BeautifySource(string editorSource)
        {
            var beautifier = Beautifier.CreateDefault();
            return beautifier.Beautify(editorSource);
        }

        private async void BtnBeautify_OnClick(object sender, RoutedEventArgs e)
        {
            var editorSource = TextEditor.Text;
            _isUnsaved = true;
            UpdateTitle();
            var beautifiedSource = await Task.Run(() => BeautifySource(editorSource));
            TextEditor.Text = beautifiedSource;
        }

        private void BtnCopyObfuscatedCode_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(OutputEditor.Text);
        }

        private void BtnExaPhaser_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://iridiumion.xyz");
        }

        private async void BtnLocalVmExecute_OnClick(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => ExecuteSourceInTextEditor());
        }

        private void BtnObfuscate_OnClick(object sender, RoutedEventArgs e)
        {
            RunJSObfuscator();
        }

        private async void CheckGitHubAvailability()
        {
            await Dispatcher.InvokeAsync(() => menuGitHub.IsEnabled = false);
            var gitHubAvailable = await _gitHubAccessProvider.CheckIfAuthenticationIsValid();
            await Dispatcher.InvokeAsync(() =>
            {
                menuGitHub.IsEnabled = _gitHubAccessProvider.ConnectionAvailable;
                if (menuGitHub.IsEnabled)
                {
                    //Connection's still there
                    menuGitHubAuth.Header = gitHubAvailable ? "Manage" : "Connect";
                    //Enable submenus
                    btnCreateGist.IsEnabled = true;
                    btnOpenGist.IsEnabled = true;
                }
            });
        }

        private async void ExecuteSourceInTextEditor()
        {
            var jsEngine = new IridiumJSEngine(cfg => { cfg.AllowClr(ExaJSInit.GetExaJSAssemblies()); });
            var console = new JSConsole(outputTb);
            await Task.Run(() => console.clear());
            jsEngine.SetValue("console", console);
            var jsSource = "";
            await Dispatcher.InvokeAsync(() =>
            {
                ConsoleTab.IsSelected = true;
                jsSource = TextEditor.Text;
            }); //Switch to output tab
            try
            {
                jsEngine.Execute(jsSource);
            }
            catch (JavaScriptException jEx)
            {
                await Task.Run(() => console.WriteLine("{0},{1} - {2}", jEx.LineNumber, jEx.Column, jEx.Error));
            }
            catch (ParserException pEx)
            {
                await Task.Run(() => console.WriteLine("{0},{1} - {2}", pEx.LineNumber, pEx.Column, pEx.Description));
            }
            catch (TargetInvocationException tEx)
            {
                await Task.Run(() => console.WriteLine(
                    $"{tEx.InnerException.GetType().Name} - {tEx.InnerException.Message}"));
            }
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

        private async void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (_forceClose) return;
            e.Cancel = true;
            await Dispatcher.BeginInvoke(new Action(async () =>
            {
                if (_isUnsaved)
                {
                    var result = await
                        this.ShowMessageAsync("Unsaved Changes",
                            "You have unsaved changes in the file you are currently editing. Do you want to save your changes?",
                            MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                            new MetroDialogSettings()
                            {
                                AffirmativeButtonText = "Save",
                                NegativeButtonText = "Don't Save",
                                FirstAuxiliaryButtonText = "Cancel"
                            });
                    switch (result)
                    {
                        case MessageDialogResult.Affirmative:
                            OnSaveFile(null, null);
                            if (!_isUnsaved)
                            {
                                _forceClose = true;
                                Close();
                            }
                            break;

                        case MessageDialogResult.Negative:
                            _forceClose = true;
                            Close();
                            break;

                        case MessageDialogResult.FirstAuxiliary:
                            e.Cancel = true;
                            break;
                    }
                }
                else
                {
                    _forceClose = true;
                    Close();
                }
            }));
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadEditorTheme();
            TextEditor.TextArea.TextEntering += TextAreaOnTextEntering;
            TextEditor.TextArea.TextEntered += TextAreaOnTextEntered;
            TextEditor.TextArea.KeyDown += TextAreaOnKeyDown;
            UpdateTitle();
            InstallEditorComponents();
            Task.Factory.StartNew(CheckGitHubAvailability);
        }

        private void InstallEditorComponents()
        {
            TextEditor.TextArea.LeftMargins.Insert(0, new BreakPointMargin());
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new About() { Owner = this }.ShowDialog();
        }

        private void OnClickExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OnCreateGist(object sender, RoutedEventArgs e)
        {
            var continueLoading = !_isUnsaved || await PromptSave();
            if (!continueLoading) return;
            var newGistName = await this.ShowInputAsync("New Gist", "Enter a name for the new Gist", new MetroDialogSettings());
            if (newGistName != null)
            {
                var pubPri =
                await
                    this.ShowMessageAsync("New Gist", "Should the gist be public or private?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Public", NegativeButtonText = "Private" });
                bool gistPublic = pubPri == MessageDialogResult.Affirmative;
                var progress = await this.ShowProgressAsync("Please Wait...", "Creating Gist...");
                progress.SetIndeterminate();
                try
                {
                    var fnInfo = new FileInfo(newGistName);
                    if (fnInfo.Extension == "")
                    {
                        newGistName += ".js";
                    }
                    var newGistRequest = new NewGist() { Description = newGistName, Public = gistPublic };
                    newGistRequest.Files[newGistName] = "//Created with Mystifier Studio\n";
                    var newGist = await _gitHubAccessProvider.ApiClient.Gist.Create(newGistRequest);
                    _currentFile = null;
                    _currentGist = newGistName;
                    _editingGist = true;
                    _currentGistId = newGist.Id;
                    _isUnsaved = true;
                    TextEditor.Text = newGist.Files[newGistName].Content;
                    UpdateTitle();
                }
                catch (Exception)
                {
                    await this.ShowMessageAsync("Error", "Gist creation failed");
                }
                await progress.CloseAsync();
            }
        }

        private void OnLoadFile(object sender, EventArgs e)
        {
            var newFile = MystifierUtil.BrowseForOpenFile("JavaScript Source Files (*.js)|*.js|All Files (*.*)|*.*",
                "Load File");
            if (newFile != null)
            {
                _editingGist = false;
                _currentGist = null;
                _currentFile = newFile;
                TextEditor.Load(_currentFile);
                _isUnsaved = false;
                UpdateTitle();
            }
        }

        private void OnNewFile(object sender, EventArgs e)
        {
            _currentFile = null;
            TextEditor.Text = "";
            _isUnsaved = true;
            UpdateTitle();
        }

        private async void OnOpenFromUrl(object sender, RoutedEventArgs e)
        {
            var url = await this.ShowInputAsync("Open from URL", "Enter a URL to open from");
            if (url != null)
            {
                var progress = await this.ShowProgressAsync("Please wait...", "Loading...");
                try
                {
                    var continueLoading = !_isUnsaved || await PromptSave();
                    if (continueLoading)
                    {
                        var wc = new WebClient();
                        progress.SetIndeterminate();
                        var code = await wc.DownloadStringTaskAsync(url);

                        TextEditor.Text = code;
                        _isUnsaved = true;
                    }
                }
                catch
                {
                    await this.ShowMessageAsync("Error", "The file could not be opened.");
                }
                await progress.CloseAsync();
            }
        }

        private async void OnOpenGist(object sender, RoutedEventArgs e)
        {
            var continueLoading = !_isUnsaved || await PromptSave();
            if (!continueLoading) return;
            var gistId = await this.ShowInputAsync("Open Gist", "Enter the ID of the Gist");
            if (gistId != null)
            {
                var progress = await this.ShowProgressAsync("Please Wait...", "Opening Gist...");
                progress.SetIndeterminate();
                try
                {
                    var gist = await _gitHubAccessProvider.ApiClient.Gist.Get(gistId);
                    var editFile = gist.Files.First();
                    _currentFile = null;
                    _currentGist = editFile.Key;
                    _editingGist = true;
                    _currentGistId = gist.Id;
                    _isUnsaved = true;
                    TextEditor.Text = editFile.Value.Content;
                    UpdateTitle();
                }
                catch (Exception)
                {
                    await this.ShowMessageAsync("Error", "Gist load failed");
                }
                await progress.CloseAsync();
            }
        }

        private void OnSaveFile(object sender, EventArgs e)
        {
            if (_currentFile != null)
            {
                TextEditor.Save(_currentFile);
                _isUnsaved = false;
                UpdateTitle();
            }
            else if (_editingGist)
            {
                Action saveGistAction = async () =>
                {
                    try
                    {
                        var gistUpdateRequest = new GistUpdate();
                        gistUpdateRequest.Files[_currentGist] = new GistFileUpdate() { Content = TextEditor.Text };
                        await _gitHubAccessProvider.ApiClient.Gist.Edit(_currentGistId, gistUpdateRequest);
                        _isUnsaved = false;
                        UpdateTitle();
                    }
                    catch
                    {
                        await this.ShowMessageAsync("Error", "Could not save gist.");
                    }
                };
                saveGistAction();
            }
            else
            {
                _currentGist = null;
                _editingGist = false;
                _currentFile = MystifierUtil.BrowseForSaveFile(
                    "JavaScript Source Files (*.js)|*.js|All Files (*.*)|*.*", "Save File");
                if (_currentFile == null)
                {
                    return;
                }
                OnSaveFile(null, null);
            }
        }

        private void OnSaveFileAs(object sender, EventArgs e)
        {
            if (_editingGist)
            {
                OnSaveFile(null, null);
                _editingGist = false;
                _currentGist = null;
                OnSaveFileAs(null, null); //Call again, but this time not editing
            }
            else
            {
                var previousFile = _currentFile;
                _currentFile = null;
                OnSaveFile(null, null);
                if (_currentFile == null)
                {
                    _currentFile = previousFile;
                }
            }
        }

        private void OutputTb_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            outputTb.ScrollToEnd();
        }

        private async Task<bool> PromptSave() //Returns false to cancel
        {
            var result = await
                        this.ShowMessageAsync("Unsaved Changes",
                            "You have unsaved changes in the file you are currently editing. Do you want to save your changes?",
                            MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                            new MetroDialogSettings()
                            {
                                AffirmativeButtonText = "Save",
                                NegativeButtonText = "Don't Save",
                                FirstAuxiliaryButtonText = "Cancel"
                            });
            switch (result)
            {
                case MessageDialogResult.Affirmative:
                    OnSaveFile(null, null);
                    if (!_isUnsaved)
                    {
                        return true;
                    }
                    break;

                case MessageDialogResult.Negative:
                    return true;

                case MessageDialogResult.FirstAuxiliary:
                    return false;
            }
            return false;
        }

        private async void RunJSObfuscator()
        {
            OutputSourceTab.IsSelected = true;
            var controller = await this.ShowProgressAsync("Please Wait", "Obfuscating Source...");
            controller.SetIndeterminate();
            var inputSource = await Dispatcher.InvokeAsync(() => TextEditor.Text);
            var obfuscatedSource = await Task.Run(() => ObfuscateJsSource(inputSource));
            await Dispatcher.BeginInvoke(new Func<string>(() => OutputEditor.Text = obfuscatedSource));
            await controller.CloseAsync();
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
                        OnSaveFileAs(null, null);
                    }
                    else
                    {
                        OnSaveFile(null, null);
                    }
                }
                if (e.Key == Key.O)
                {
                    OnLoadFile(null, null);
                }
                if (e.Key == Key.N)
                {
                    OnNewFile(null, null);
                }
            }
            if (e.Key == Key.F5)
            {
                ExecuteSourceInTextEditor();
            }
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if ((Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) && Keyboard.IsKeyDown(Key.F))
                {
                    e.Handled = true;
                    BtnBeautify_OnClick(null, null);
                }
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

        private async void ToggleGitHubAuth(object sender, RoutedEventArgs e)
        {
            if (_gitHubAccessProvider.GitHubAuthenticationValid)
            {
                //Disassociate with GitHub
                var currentUser = await _gitHubAccessProvider.ApiClient.User.Current();
                var choice = await
                    this.ShowMessageAsync("GitHub Authentication",
                        $"You are signed in to GitHub as {currentUser.Login}.", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "OK", NegativeButtonText = "Disconnect" });
                if (choice == MessageDialogResult.Negative)
                {
                    choice = await this.ShowMessageAsync("GitHub Authentication",
                        "Are you sure you want to disconnect? You will no longer be able to use the GitHub integration features.", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });
                    if (choice == MessageDialogResult.Affirmative)
                    {
                        _gitHubAccessProvider.DiscardSavedCredentials();
                        _gitHubAccessProvider = new MystifierGitHubAccess(); //Reinitialize GitHub access
                        await Task.Factory.StartNew(CheckGitHubAvailability);
                        await
                            this.ShowMessageAsync("GitHub Authentication",
                                "You have successfully disconnected Mystifier Studio from GitHub.");
                    }
                }
            }
            else
            {
                //Connect to GitHub
                Credentials gitHubCreds = null;
                var result = await
                    this.ShowMessageAsync("GitHub Authentication",
                        "Would you like to connect to GitHub with an access token or your login credentials?",
                        MessageDialogStyle.AffirmativeAndNegative,
                        new MetroDialogSettings() { AffirmativeButtonText = "Access Token", NegativeButtonText = "Login" });
                switch (result)
                {
                    case MessageDialogResult.Affirmative: //Auth token
                        var authToken = await this.ShowInputAsync("Connect to GitHub", "Please enter an authorization token");
                        if (authToken != null)
                            gitHubCreds = new Credentials(authToken);
                        break;

                    case MessageDialogResult.Negative: //Creds
                        var credentialText =
                            await
                                this.ShowLoginAsync("Log in to GitHub", "Please enter your GitHub credentials", new LoginDialogSettings() { RememberCheckBoxVisibility = Visibility.Hidden });
                        if (credentialText.Username != null && credentialText.Password != null)
                        {
                            var gitHubUsername = credentialText.Username.TrimEnd('\r', '\n');
                            var githubPassword = credentialText.Password.TrimEnd('\r', '\n');
                            gitHubCreds = new Credentials(gitHubUsername, githubPassword);
                        }
                        break;
                }
                if (gitHubCreds != null)
                {
                    _gitHubAccessProvider.LoadCredentials(gitHubCreds);
                    //Reload GitHub Access
                    var progress = await this.ShowProgressAsync("Please wait...", "Authenticating...");
                    progress.SetIndeterminate();
                    var success = await _gitHubAccessProvider.CheckIfAuthenticationIsValid();
                    if (success)
                    {
                        await
                            this.ShowMessageAsync("Success!",
                                "You have successfully connected Mystifier Studio to GitHub!");
                        await Task.Run(() => CheckGitHubAvailability());
                    }
                    else
                    {
                        await
                            this.ShowMessageAsync("Error", "Sorry, Mystifier Studio could not log in to GitHub with the provided credentials.");
                    }
                    await progress.CloseAsync();
                }
            }
        }

        private void UpdateTitle()
        {
            if (_currentFile == null && !_editingGist)
            {
                tbFileName.Text = _isUnsaved ? "[New File*]" : "[New File]";
            }
            else if (_editingGist)
            {
                tbFileName.Text = "Gist: " + (_isUnsaved ? _currentGist + "*" : _currentGist) + $" ({_currentGistId})";
            }
            else
            {
                var fn = Path.GetFileName(_currentFile);
                tbFileName.Text = _isUnsaved ? fn + "*" : fn;
            }
        }

        #endregion Private Methods
    }
}
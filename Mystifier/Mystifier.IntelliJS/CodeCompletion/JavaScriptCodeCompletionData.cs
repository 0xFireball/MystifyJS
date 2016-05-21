using System;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Mystifier.IntelliJS.CodeCompletion
{
    public class JavaScriptCodeCompletionData : ICompletionData
    {
        #region Public Constructors

        public JavaScriptCodeCompletionData(string text, string description, int priority = 0)
        {
            Text = text;
            DescriptionText = description;
            Priority = priority;
        }

        #endregion Public Constructors

        #region Public Properties

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => this.Text;

        public object Description => this.DescriptionText;
        public string DescriptionText { get; set; }
        public System.Windows.Media.ImageSource Image => null;

        public double Priority { get; }
        public string Text { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs) => textArea.Document.Replace(completionSegment, this.Text);

        #endregion Public Methods
    }
}
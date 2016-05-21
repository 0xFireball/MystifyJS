using System;
using System.Windows.Media;
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

        #region Public Methods

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs) => textArea.Document.Replace(completionSegment, Text);

        #endregion Public Methods

        #region Public Properties

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => Text;

        public object Description => DescriptionText;
        public string DescriptionText { get; set; }
        public ImageSource Image => null;

        public double Priority { get; }
        public string Text { get; }

        #endregion Public Properties
    }
}
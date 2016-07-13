using Android.Text;
using Java.Lang;

namespace MystifierLightEditor.Controls.Internal
{
    internal class EditorTextWatcher : Java.Lang.Object, ITextWatcher
    {
        private int twstart = 0;
        private int twcount = 0;
        private IridiumHighlightingEditor _editor;

        public EditorTextWatcher(IridiumHighlightingEditor editor)
        {
            _editor = editor;
        }

        public void AfterTextChanged(IEditable ed)
        {
            _editor.CancelUpdate();
            _editor.ConvertTabs(ed, twstart, twcount);

            if (!_editor.Modified)
                return;

            _editor.Dirty = true;
            _editor.UpdateHandler.PostDelayed(
                _editor.UpdateRunnable,
                _editor.UpdateDelay);
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            this.twstart = start;
            this.twcount = count;
        }
    }
}
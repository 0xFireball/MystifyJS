using Android.Text;
using Java.Lang;

namespace MystifierLightEditor.Controls.Internal
{
    internal class NewlineInputFilter : Java.Lang.Object, IInputFilter
    {
        private IridiumHighlightingEditor _editor;

        public NewlineInputFilter(IridiumHighlightingEditor editor)
        {
            _editor = editor;
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            if (_editor.Modified && end - start == 1 && start < source.Length() && dstart < dest.Length())
            {
                var c = source.CharAt(start);
                if (c == '\n')
                {
                    return _editor.AutoIndent(source, dest, dstart, dend);
                }
            }
            return source;
        }
    }
}
using Android.Text;
using Java.Lang;

namespace MystifierLightEditor.Classes
{
    internal class NewlineInputFilter : Java.Lang.Object, IInputFilter
    {
        private object _modifiedSignal;

        public NewlineInputFilter(object modifiedSignal)
        {
            _modifiedSignal = modifiedSignal;
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            var modified = (bool)_modifiedSignal;
            if (modified && end - start == 1 && start < source.Length() && dstart < dest.Length())
            {
                var c = source.CharAt(start);
                if (c == '\n')
                {
                    return AutoIndent(source, dest, dstart, dend);
                }
            }
            return source;
        }

        private ICharSequence AutoIndent(
        ICharSequence source,
        ISpanned dest,
        int dstart,
        int dend)
        {
            string indent = "";
            int istart = dstart - 1;
            int iend = -1;

            // find start of this line
            bool dataBefore = false;
            int pt = 0;

            for (; istart > -1; --istart)
            {
                var c = dest.CharAt(istart);

                if (c == '\n')
                    break;

                if (c != ' ' &&
                    c != '\t')
                {
                    if (!dataBefore)
                    {
                        // indent always after those characters
                        if (c == '{' ||
                            c == '+' ||
                            c == '-' ||
                            c == '*' ||
                            c == '/' ||
                            c == '%' ||
                            c == '^' ||
                            c == '=')
                            --pt;

                        dataBefore = true;
                    }

                    // parenthesis counter
                    if (c == '(')
                        --pt;
                    else if (c == ')')
                        ++pt;
                }
            }

            // copy indent of this line into the next
            if (istart > -1)
            {
                var charAtCursor = dest.CharAt(dstart);

                for (iend = ++istart;
                    iend < dend;
                    ++iend)
                {
                    var c = dest.CharAt(iend);

                    // auto expand comments
                    if (charAtCursor != '\n' &&
                        c == '/' &&
                        iend + 1 < dend &&
                        dest.CharAt(iend) == c)
                    {
                        iend += 2;
                        break;
                    }

                    if (c != ' ' &&
                        c != '\t')
                        break;
                }

                indent += dest.SubSequence(istart, iend);
            }

            // add new indent
            if (pt < 0)
                indent += "\t";

            // append white space of previous line and new indent
            return new Java.Lang.String(source.ToString() + new Java.Lang.String(indent));
        }
    }
}
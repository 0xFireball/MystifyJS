using System;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Widget;
using Java.Lang;
using Java.Util.Regex;
using MystifierLightEditor.Controls.Internal;
using MystifierLightEditor.SyntaxHighlighting;

namespace MystifierLightEditor.Controls
{
    public class IridiumHighlightingEditor : EditText
    {
        private IOnTextChangedListener onTextChangedListener;

        public PatternBasedHighlightingDefinition HighlightingDefinition { get; set; }
        public Handler UpdateHandler { get; set; }
        public Runnable UpdateRunnable { get; set; }
        public int ErrorLine { get; set; } = 0;
        public bool Dirty { get; set; } = false;
        public bool Modified { get; set; } = true;
        public int TabWidth { get; set; } = 4;
        public long UpdateDelay { get; private set; } = 1000;

        public IridiumHighlightingEditor(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(context);
            HighlightingDefinition = new PatternBasedHighlightingDefinition();
        }

        public void SetOnTextChangedListener(IOnTextChangedListener listener)
        {
            onTextChangedListener = listener;
        }

        protected void Initialize(Context context)
        {
            UpdateHandler = new Handler();
            UpdateRunnable = new Runnable(() =>
            {
                var ed = EditableText;
                onTextChangedListener?.OnTextChanged(ed.ToString());
                HighlightWithoutChange(ed);
            });
            SetHorizontallyScrolling(true);
            SetFilters(new IInputFilter[] { new NewlineInputFilter(this) });
            AddTextChangedListener(new EditorTextWatcher(this));
        }

        public void CancelUpdate()
        {
            UpdateHandler.RemoveCallbacks(UpdateRunnable);
        }

        private void HighlightWithoutChange(IEditable ed)
        {
            Modified = false;
            Highlight(ed);
            Modified = true;
        }

        private IEditable Highlight(IEditable e)
        {
            try
            {
                ClearSpans(e);
                if (e.Length() == 0)
                    return e;
                if (ErrorLine > 0)
                {
                    Matcher m = HighlightingDefinition.LinePattern.Matcher(e);
                    for (int n = ErrorLine;
                    n-- > 0 && m.Find();) ;

                    e.SetSpan(
                        new BackgroundColorSpan(HighlightingDefinition.ErrorColor),
                        m.Start(),
                        m.End(),
                        SpanTypes.ExclusiveExclusive);
                }

                for (Matcher m = HighlightingDefinition.NumbersPattern.Matcher(e);
                m.Find();)
                    e.SetSpan(
                        new ForegroundColorSpan(HighlightingDefinition.NumberColor),
                        m.Start(),
                        m.End(),
                        SpanTypes.ExclusiveExclusive);

                for (Matcher m = HighlightingDefinition.StringPattern.Matcher(e);
                m.Find();)
                    e.SetSpan(
                        new ForegroundColorSpan(HighlightingDefinition.StringColor),
                        m.Start(),
                        m.End(),
                        SpanTypes.ExclusiveExclusive);

                for (Matcher m = HighlightingDefinition.KeywordPattern.Matcher(e);
                m.Find();)
                    e.SetSpan(
                        new ForegroundColorSpan(HighlightingDefinition.KeywordColor),
                        m.Start(),
                        m.End(),
                        SpanTypes.ExclusiveExclusive);

                for (Matcher m = HighlightingDefinition.BuiltinsPattern.Matcher(e);
                m.Find();)
                    e.SetSpan(
                        new ForegroundColorSpan(HighlightingDefinition.BuiltinsColor),
                        m.Start(),
                        m.End(),
                        SpanTypes.ExclusiveExclusive);

                for (Matcher m = HighlightingDefinition.CommentsPattern.Matcher(e);
                m.Find();)
                    e.SetSpan(
                        new ForegroundColorSpan(HighlightingDefinition.CommentsColor),
                        m.Start(),
                        m.End(),
                        SpanTypes.ExclusiveExclusive);
            }
            catch (IllegalStateException)
            {
                // raised by Matcher.start()/.end() when
                // no successful match has been made what
                // shouldn't ever happen because of find()
            }
            return e;
        }

        public void ClearSpans(IEditable e)
        {
            // remove foreground color spans
            var spans = e.GetSpans(
                      0,
                      e.Length(),
                      Class.FromType(typeof(ForegroundColorSpan)));
            for (int n = spans.Length; n-- > 0;)
                e.RemoveSpan(spans[n]);
        }

        internal ICharSequence AutoIndent(
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

        internal void ConvertTabs(IEditable e, int start, int count)
        {
            if (TabWidth < 1)
                return;

            string s = e.ToString();

#pragma warning disable RECS0130 // 'for' loop control variable is never modified
            for (int stop = start + count;
#pragma warning restore RECS0130 // 'for' loop control variable is never modified
                (start = s.IndexOf("\t", start, StringComparison.CurrentCulture)) > -1 && start < stop;
                start++)
                e.SetSpan(
                    new TabWidthSpan(this),
                    start,
                    start + 1,
                    SpanTypes.ExclusiveExclusive);
        }

        public void LoadHighlightingDefinition(PatternBasedHighlightingDefinition highlightingDefinition)
        {
            HighlightingDefinition = highlightingDefinition;
        }
    }
}
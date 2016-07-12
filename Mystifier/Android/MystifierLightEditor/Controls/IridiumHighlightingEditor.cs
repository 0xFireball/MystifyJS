using System;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Widget;
using Java.Lang;
using MystifierLightEditor.Classes;
using MystifierLightEditor.SyntaxHighlighting;

namespace MystifierLightEditor.Controls
{
    public interface IOnTextChangedListener
    {
        void OnTextChanged(string text);
    }

    public class IridiumHighlightingEditor : EditText
    {
        private RegexHighlightingDefinition _highlightingDefinition;
        private Handler updateHandler;
        private Runnable updateRunnable;
        private static object Editable;
        private IOnTextChangedListener onTextChangedListener;
        private int updateDelay = 1000;
        private int errorLine = 0;
        private bool dirty = false;
        private bool modified = true;
        private int colorError;
        private int colorNumber;
        private int colorKeyword;
        private int colorBuiltin;
        private int colorComment;
        private int tabWidthInCharacters = 0;
        private int tabWidth = 0;


        public IridiumHighlightingEditor(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(context);
        }

        protected void Initialize(Context context)
        {
            updateHandler = new Handler();
            updateRunnable = new Runnable(() =>
            {
                var ed = EditableText;
                onTextChangedListener?.OnTextChanged(ed.ToString());
                HighlightWithoutChange(ed);
            });
            SetHorizontallyScrolling(true);
            SetFilters(new IInputFilter[] { new NewlineInputFilter((object)modified) });
        }

        private void HighlightWithoutChange(IEditable ed)
        {
            throw new NotImplementedException();
        }

        public void LoadHighlightingDefinition(RegexHighlightingDefinition highlightingDefinition)
        {
            _highlightingDefinition = highlightingDefinition;
        }
    }
}
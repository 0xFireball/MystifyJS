using System;
using Android.Graphics;
using Android.Text.Style;
using Java.Lang;

namespace MystifierLightEditor.Controls.Internal
{
    class TabWidthSpan : ReplacementSpan
    {
        private IridiumHighlightingEditor _editor;

        public TabWidthSpan(IridiumHighlightingEditor editor)
        {
            _editor = editor;
        }

        public override int GetSize(
            Paint paint,
            ICharSequence text,
            int start,
            int end,
            Paint.FontMetricsInt fm)
        {
            return _editor.TabWidth;
        }

        public override void Draw(
            Canvas canvas,
            ICharSequence text,
            int start,
            int end,
            float x,
            int top,
            int y,
            int bottom,
            Paint paint)
        {
        }
    }
}
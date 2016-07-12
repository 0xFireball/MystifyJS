using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace MystifierLightEditor.Controls
{
    public class IridiumHighlightingEditor : EditText
    {
        public IridiumHighlightingEditor(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public IridiumHighlightingEditor(Context context, IAttributeSet attrs) : base(context, attrs)
        {   
        }
    }
}
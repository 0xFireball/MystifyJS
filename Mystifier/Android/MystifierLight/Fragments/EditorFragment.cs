using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MystifierLightEditor.Controls;
using MystifierLightEditor.SyntaxHighlighting;

namespace MystifierLight.Fragments
{
    public class EditorFragment : Fragment
    {
        private IridiumHighlightingEditor _editor;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var fragmentLayoutView = inflater.Inflate(Resource.Layout.EditorFragment, container, false);
            _editor = fragmentLayoutView.FindViewById<IridiumHighlightingEditor>(Resource.Id.jsEditor);
            _editor.SetOnTextChangedListener((IOnTextChangedListener)Activity);
            return fragmentLayoutView;
        }

        private int GetYOffset(Activity activity)
        {
            int yOffset = 0;
            if (yOffset == 0)
            {
                float dp = Resources.DisplayMetrics.Density;

                try
                {
                    ActionBar actionBar = activity.ActionBar;

                    if (actionBar != null)
                        yOffset = actionBar.Height;
                }
                catch (ClassCastException e)
                {
                    yOffset = Java.Lang.Math.Round(48f * dp);
                }

                yOffset += Java.Lang.Math.Round(16f * dp);
            }

            return yOffset;
        }
    }
}
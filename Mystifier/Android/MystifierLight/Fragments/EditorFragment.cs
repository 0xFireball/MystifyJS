using Android.App;
using Android.OS;
using Android.Views;
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
    }
}
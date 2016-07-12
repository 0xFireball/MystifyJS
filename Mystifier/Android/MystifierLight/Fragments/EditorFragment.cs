using System;

using Android.App;
using Android.OS;
using Android.Views;
using MystifierLightEditor.Controls;

namespace MystifierLight.Fragments
{
    public class EditorFragment : Fragment
    {
        public IridiumHighlightingEditor Editor { get; internal set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Editor = View.FindViewById<IridiumHighlightingEditor>(Resource.Id.jsEditor);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.EditorFragment, container, false);
        }
    }
}
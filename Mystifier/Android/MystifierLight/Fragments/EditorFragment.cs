using Android.App;
using Android.OS;
using Android.Views;

namespace MystifierLight.Fragments
{
    public class EditorFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var fragmentLayoutView = inflater.Inflate(Resource.Layout.EditorFragment, container, false);
            return fragmentLayoutView;
        }
    }
}
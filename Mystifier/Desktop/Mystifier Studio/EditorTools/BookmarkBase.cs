using System;
using System.Windows.Media.Imaging;

namespace Mystifier.EditorTools
{
    internal class BookmarkBase
    {
        public static System.Windows.Controls.Image DefaultBookmarkImage
        {
            get { return LoadImageSource("pack://application:,,,/Resources.defaultbookmark.png"); }
        }

        private static System.Windows.Controls.Image LoadImageSource(string resourceName)
        {
            var img = new System.Windows.Controls.Image();
            //var imgStrm = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName));
            img.Source = new BitmapImage(new Uri(resourceName));
            return img;
        }
    }
}
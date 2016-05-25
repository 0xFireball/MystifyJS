using System.Net;
using Microsoft.Win32;

namespace Mystifier
{
    internal class MystifierUtil
    {
        public static string BrowseForSaveFile(string filter, string title)
        {
            var sfd = new SaveFileDialog
            {
                Filter = filter,
                Title = title
            };
            var showDialog = sfd.ShowDialog();
            if (showDialog != null && (bool)showDialog)
            {
                return sfd.FileName;
            }
            return null;
        }

        public static string BrowseForOpenFile(string filter, string title)
        {
            var ofd = new OpenFileDialog
            {
                Filter = filter,
                Title = title,
                Multiselect = false
            };
            var showDialog = ofd.ShowDialog();
            if (showDialog != null && (bool)showDialog)
            {
                return ofd.FileName;
            }
            return null;
        }

        public static bool IsInternetConnectionAvailable()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("https://www.google.com")) //Check Google
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
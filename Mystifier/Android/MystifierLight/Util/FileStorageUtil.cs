using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MystifierLight.Util
{
    static class FileStorageUtil
    {
        public static string GetFileAbsolutePath(string localFilePath)
        {
            return Path.Combine(Android.OS.Environment.DataDirectory.AbsolutePath, localFilePath);
        }
    }
}
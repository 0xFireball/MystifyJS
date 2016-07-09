using System;
using Android.App;
using Android.Content;
using Android.Widget;

namespace MystifierLight.Util
{
    internal class DialogUtil
    {
        public static void ShowInputDialog(Context context, string title, Action<string> onComplete)
        {
            string ret = null;
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(context);
            dialogBuilder.SetTitle(title);
            EditText inputBox = new EditText(context);
            inputBox.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationUri;
            dialogBuilder.SetView(inputBox);
            dialogBuilder.SetPositiveButton("OK", (s, e) =>
            {
                ret = inputBox.Text;
                onComplete(ret);
            });
            dialogBuilder.SetNegativeButton("Cancel", (s, e) => { onComplete(ret); });
            dialogBuilder.Show();
        }
    }
}
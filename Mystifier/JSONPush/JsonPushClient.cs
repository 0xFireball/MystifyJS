using Android.App;
using Android.Content;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace JSONPush
{
    public class JsonPushClient
    {
        public string FeedSource { get; }
        public MessageFeed DownloadedFeed;
        private ReadMessageStore ReadMessages = new ReadMessageStore();

        public JsonPushClient(string remoteResourceUrl)
        {
            FeedSource = remoteResourceUrl;
        }

        public async Task<bool> FetchFeed()
        {
            try
            {
                var wc = new WebClient();
                var feedContents = await wc.DownloadStringTaskAsync(FeedSource);
                DownloadedFeed = JsonConvert.DeserializeObject<MessageFeed>(feedContents);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void DisplayPendingMessages(Context context)
        {
            if (DownloadedFeed == null) throw new ArgumentNullException(nameof(DownloadedFeed));
            foreach (var msg in DownloadedFeed.Information)
            {
                if (!ReadMessages.CurrentReadMessageIds.Contains(msg.MessageHash))
                {
                    ReadMessages.AddReadMessage(msg);
                    var messageAlert = new AlertDialog.Builder(context);
                    messageAlert.SetTitle(msg.Title);
                    messageAlert.SetCancelable(false);
                    messageAlert.SetPositiveButton("Cool", (s, e) => { });
                    var dialog = messageAlert.Create();
                    dialog.Show();
                }
            }
        }
    }
}
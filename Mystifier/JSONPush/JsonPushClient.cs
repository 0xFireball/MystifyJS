using Android.App;
using Newtonsoft.Json;
using System;
using System.Linq;
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

        public void DisplayPendingMessages(Activity activity)
        {
            if (DownloadedFeed == null) throw new ArgumentNullException(nameof(DownloadedFeed));
            var downloadedMessageList = DownloadedFeed.Information;
            var unreadMessages = downloadedMessageList.Where(msg => !ReadMessages.CurrentReadMessageIds.Contains(msg.MessageHash));
            foreach (var msg in unreadMessages)
            {
                ReadMessages.AddReadMessage(msg);
                activity.RunOnUiThread(() =>
                {
                    var messageAlert = new AlertDialog.Builder(activity);
                    messageAlert.SetTitle(msg.Title);
                    messageAlert.SetCancelable(false);
                    messageAlert.SetPositiveButton("Cool", (s, e) => { });
                    var dialog = messageAlert.Create();
                    dialog.Show();
                });
            }
        }
    }
}
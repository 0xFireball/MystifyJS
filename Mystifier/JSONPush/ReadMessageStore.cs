using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace JSONPush
{
    internal class ReadMessageStore
    {
        public string PhysicalLocation => Path.Combine(Android.OS.Environment.DataDirectory.AbsolutePath, "readmessages.store");
        public List<string> CurrentReadMessageIds;

        public void AddReadMessage(PushMessage message)
        {
            CurrentReadMessageIds.Add(message.MessageHash);
            SaveStore();
        }

        public ReadMessageStore()
        {
            LoadStore();
        }

        private void LoadStore()
        {
            var serMsgStore = File.ReadAllText(PhysicalLocation);
            CurrentReadMessageIds = JsonConvert.DeserializeObject<List<string>>(serMsgStore);
        }

        private void SaveStore()
        {
            File.WriteAllText(PhysicalLocation, JsonConvert.SerializeObject(CurrentReadMessageIds));
        }
    }
}
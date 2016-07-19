using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace JSONPush
{
    internal class ReadMessageStore
    {
        public string PhysicalLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "readmessages.store");
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
            if (File.Exists(PhysicalLocation))
            {

                var serMsgStore = File.ReadAllText(PhysicalLocation);
                CurrentReadMessageIds = JsonConvert.DeserializeObject<List<string>>(serMsgStore);
            }
            else
            {
                CurrentReadMessageIds = new List<string>();
                SaveStore();
            }
        }

        private void SaveStore()
        {
            File.WriteAllText(PhysicalLocation, JsonConvert.SerializeObject(CurrentReadMessageIds));
        }
    }
}
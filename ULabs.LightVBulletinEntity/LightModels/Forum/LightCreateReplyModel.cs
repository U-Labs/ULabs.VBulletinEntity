using System;
using System.Collections.Generic;
using System.Text;
using ULabs.LightVBulletinEntity.LightModels.User;

namespace ULabs.LightVBulletinEntity.LightModels.Forum {
    public class LightCreateReplyModel {
        /// <summary>
        /// Used author instead of user id because we need access to the users group for permission checks
        /// </summary>
        public VBLightUser Author { get; set; }
        public int ThreadId { get; set; }
        public int ForumId { get; set; }
        public string Text { get; set; }
        public string IpAddress { get; set; }
        public string Title { get; set; }
        public long? TimeRaw { get; set; }
        public bool UpdateCounters { get; set; }
        public LightCreateReplyModel(VBLightUser author, int forumId, int threadId, string text, string ipAddress, string title = "", long? timeRaw = null, bool updateCounters = true) {
            Author = author;
            ForumId = forumId;
            ThreadId = threadId;
            Text = text;
            IpAddress = ipAddress;
            Title = title;
            TimeRaw = timeRaw;
            UpdateCounters = updateCounters;
        }
    }
}

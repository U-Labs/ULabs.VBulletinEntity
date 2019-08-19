using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightModels {
    public class VBLightUnreadActiveThread {
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
        public int ForumId { get; set; }
        public string ForumTitle { get; set; }
        public int LastPostTimeRaw { get; set; }
        public int LastPosterUserId { get; set; }
        public int LastPosterAvatarRevision { get; set; }

        public DateTime LastPostTime {
            get => LastPostTimeRaw.ToDateTime();
            set => LastPostTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

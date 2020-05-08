using System;
using System.Collections.Generic;
using System.Text;
using ULabs.LightVBulletinEntity.Tools;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.LightVBulletinEntity.LightModels {
    public class VBLightUnreadActiveThread {
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
        public int ForumId { get; set; }
        public string ForumTitle { get; set; }
        public int LastPostTimeRaw { get; set; }
        public int LastPosterUserId { get; set; }
        public int LastPosterAvatarRevision { get; set; }
        public bool LastPosterHasAvatar { get; set; }
        public DateTime LastPostTime {
            get => LastPostTimeRaw.ToDateTime();
            set => LastPostTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

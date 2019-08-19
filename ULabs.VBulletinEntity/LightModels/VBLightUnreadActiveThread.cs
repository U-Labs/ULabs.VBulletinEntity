using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightModels {
    public class VBLightUnreadActiveThread {
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
        public int LastThreadReadTimeRaw { get; set; }
        public int ForumId { get; set; }
        public string ForumTitle { get; set; }

        public DateTime LastThreadReadTime {
            get => LastThreadReadTimeRaw.ToDateTime();
            set => LastThreadReadTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

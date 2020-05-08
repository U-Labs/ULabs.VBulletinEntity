using System;
using System.Collections.Generic;
using System.Text;
using ULabs.LightVBulletinEntity.Tools;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.LightVBulletinEntity.LightModels.Forum {
    public class VBLightPostThanks {
        public int PostId { get; set; }
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
        public int ForumId { get; set; }
        public string ForumTitle { get; set; }
        public string AuthorName { get; set; }
        public string AuthorGroupOpenTag { get; set; }
        public string AuthorGroupCloseTag { get; set; }
        public int TimeRaw { get; set; }

        public DateTime Time {
            get => TimeRaw.ToDateTime();
            set => TimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

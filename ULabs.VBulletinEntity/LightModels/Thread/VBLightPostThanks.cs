﻿using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightModels.Thread {
    public class VBLightPostThanks {
        public int PostId { get; set; }
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
        public int ForumId { get; set; }
        public string ForumTitle { get; set; }
        public int TimeRaw { get; set; }

        public DateTime Time {
            get => TimeRaw.ToDateTime();
            set => TimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

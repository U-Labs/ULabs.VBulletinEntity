using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.VBulletinEntity.LightModels.User {
    public class VBLightLoginStrike {
        public int TimeRaw { get; set; }
        public DateTime Time {
            get => TimeRaw.ToDateTime();
            set => TimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        public string IpAddress { get; set; }
        public string UserName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightModels {
    public class VBLightSession {
        public string SessionHash { get; set; }

        public int UserId { get; set; }
        public VBLightUser User { get; set; }
        public string Host { get; set; }
        public string IdHash { get; set; }
        public int LastActivityRaw { get; set; }
        public string Location { get; set; }
        public string UserAgent { get; set; }
        public int LoggedInRaw { get; set; }
        public bool IsBot { get; set; }

        public DateTime LastActivity {
            get => DateTimeExtensions.ToDateTime(LastActivityRaw);
            set => LastActivityRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
        public bool IsLoggedIn {
            get { return LoggedInRaw == 2 || LoggedInRaw == 1; }
        }
    }
}

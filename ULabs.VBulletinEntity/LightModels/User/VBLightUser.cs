using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightModels.User {
    public class VBLightUser {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserTitle { get; set; }
        public VBLightUserGroup PrimaryUserGroup { get; set; }
        public int AvatarRevision { get; set; }
        public bool HasAvatar { get; set; }
        public int LastActivityRaw { get; set; }
        public int UnreadPmsCount { get; set; }
        public int UnreadThanksCount { get; set; }
        public int PostsCount { get; set; }
        public DateTime LastActivity {
            get => DateTimeExtensions.ToDateTime(LastActivityRaw);
            set => LastActivityRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

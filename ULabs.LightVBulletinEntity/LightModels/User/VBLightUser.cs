using System;
using System.Collections.Generic;
using System.Text;
using ULabs.LightVBulletinEntity.Tools;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.LightVBulletinEntity.LightModels.User {
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
        public string UserNameFormatted {
            get => $"{PrimaryUserGroup.OpenTag}{UserName}{PrimaryUserGroup.CloseTag}";
        }
        public DateTime LastActivity {
            get => DateTimeExtensions.ToDateTime(LastActivityRaw);
            set => LastActivityRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

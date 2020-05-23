using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels.User {
    public class LightTopPoster {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int IntervalPosts { get; set; }
        public int TotalPosts { get; set; }
        public string OpenTag { get; set; }
        public string CloseTag { get; set; }
        public int AvatarRevision { get; set; }
        public bool HasAvatar { get; set; }

        public string UserNameFormatted {
            get => $"{OpenTag}{UserName}{CloseTag}";
        }
    }
}

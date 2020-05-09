using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.LightModels.User;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightModels.Forum {
    public class VBLightPost {
        public int Id { get; set; }
        public int ParentPostId { get; set; }
        public int ThreadId { get; set; }
        public VBLightUser Author { get; set; }
        public int CreatedTimeRaw { get; set; }
        public string Text { get; set; }
        public string IpAddress { get; set; }
        public int VisibilityRaw { get; set; }
        public bool HasAttachments { get; set; }
        public int ThanksCount { get; set; }

        public VBPostVisibleState Visibility {
            get => (VBPostVisibleState)VisibilityRaw;
            set => VisibilityRaw = (int)value;
        }

        public DateTime CreatedTime {
            get => CreatedTimeRaw.ToDateTime();
            set => CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

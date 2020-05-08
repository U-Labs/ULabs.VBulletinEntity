using System;
using System.Collections.Generic;
using System.Text;
using ULabs.LightVBulletinEntity.LightModels.User;
using ULabs.LightVBulletinEntity.Tools;
using ULabs.VBulletinEntity.Shared.Forum;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.LightVBulletinEntity.LightModels.Forum {
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

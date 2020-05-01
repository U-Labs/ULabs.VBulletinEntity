using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.LightModels.Forum;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightModels.Moderation {
    public class VBLightDeletionLog {
        public int ContentId { get; set; }
        public DeletionLogType Type { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public int TimeRaw { get; set; }
        public DateTime Time {
            get => DateTimeExtensions.ToDateTime(TimeRaw);
            set => TimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

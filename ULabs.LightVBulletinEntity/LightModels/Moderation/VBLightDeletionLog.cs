using System;
using System.Collections.Generic;
using System.Text;
using ULabs.LightVBulletinEntity.LightModels.Forum;
using ULabs.LightVBulletinEntity.Tools;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.LightVBulletinEntity.LightModels.Moderation {
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
        public int PostPublishTimeRaw { get; set; }
        public DateTime PostPublishTime {
            get => DateTimeExtensions.ToDateTime(PostPublishTimeRaw);
            set => PostPublishTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

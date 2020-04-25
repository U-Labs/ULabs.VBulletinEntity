using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightModels.Forum {
    public class VBLightAttachment {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TimeRaw { get; set; }
        public DateTime Time {
            get => TimeRaw.ToDateTime();
            set => TimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
        public int DownloadsCount { get; set; }
        public int ContentId { get; set; }
        public string FileName { get; set; }

        #region Filedata columns
        public int FileSize { get; set; }
        public int Refcount { get; set; }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels.Forum {
    public class VBLightAttachment {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TimeRaw { get; set; }
        public int Counter { get; set; }
        public int ContentId { get; set; }
        public string Filename { get; set; }

        #region Filedata columns
        public int Filesize { get; set; }
        public int Refcount { get; set; }
        #endregion
    }
}

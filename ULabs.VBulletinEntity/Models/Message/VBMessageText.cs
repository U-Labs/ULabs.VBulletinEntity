using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ULabs.VBulletinEntity.Models.User;
using ULabs.VBulletinEntity.Shared.Tools;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.Message {
    [Table("pmtext")]
    public class VBMessageText {
        [Column("pmtextid")]
        public int Id { get; set; }

        public int FromUserId { get; set; }
        public VBUser FromUser { get; set; }

        public string FromUserName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        [Column("touserarray")]
        public string ToUserArrayRaw { get; set; }

        public int IconId { get; set; }

        [Column("dateline")]
        public int CreatedTimeRaw { get; set; }

        [NotMapped]
        public DateTime CreatedTime {
            get => CreatedTimeRaw.ToDateTime();
            set => CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        public bool ShowSignature { get; set; }

        [Column("allowsmilie")]
        public bool AllowSmilies { get; set; }

        public int ReportThreadId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ULabs.VBulletinEntity.Models.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("threadread")]
    public class VBThreadRead {
        [Key, Column(Order = 1)]
        public int UserId { get; set; }
        public VBUser User { get; set; }

        [Key, Column(Order = 2)]
        public int ThreadId { get; set; }
        public VBThread Thread { get; set; }

        [Column("readtime")]
        public int ReadTimeRaw { get; set; }

        [NotMapped]
        public DateTime ReadTime {
            get => ReadTimeRaw.ToDateTime();
            set => ReadTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        public VBThreadRead() { }

        public VBThreadRead(int userId, int threadId, DateTime? readTime = null) {
            UserId = userId;
            ThreadId = threadId;

            if (!readTime.HasValue)
                readTime = DateTime.UtcNow;

            ReadTime = readTime.Value;
        }
    }
}

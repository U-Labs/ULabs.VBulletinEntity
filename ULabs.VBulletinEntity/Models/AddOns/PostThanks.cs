using System;
using System.ComponentModel.DataAnnotations.Schema;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Models.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.AddOns {
    [Table("post_thanks")]
    public class PostThanks {
        public int Id { get; set; }

        public VBUser User { get; set; }
        public int UserId { get; set; }

        public string UserName { get; set; }

        [Column("date")]
        public int CreatedTimeRaw { get; set; }

        [NotMapped]
        public DateTime CreatedTime {
            get { return CreatedTimeRaw.ToDateTime(); }
            set { CreatedTimeRaw = value.ToUnixTimestampAsInt(); }
        }

        public int PostId { get; set; }
    }
}

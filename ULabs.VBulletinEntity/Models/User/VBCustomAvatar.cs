using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.User {
    [Table("customavatar")]
    public class VBCustomAvatar {
        [Key]
        public int UserId { get; set; }

        [Column("dateline")]
        public int CreatedTimeRaw { get; set; }

        [NotMapped]
        public DateTime CreatedTime {
            get => DateTimeExtensions.ToDateTime(CreatedTimeRaw);
            set => CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        [Column("visible")]
        public int IsVisible { get; set; }

        public int FileSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        // ToDo: Handle filedata when storing avatars in db
    }
}

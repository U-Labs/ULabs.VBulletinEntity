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

        [Column("filedata", TypeName = "MEDIUMBLOB")]
        public byte[] FileData { get; set; }

        [Column("dateline")]
        public int CreatedTimeRaw { get; set; }

        [MaxLength(100)]
        public string FileName { get; set; }

        [Column("visible")]
        public int Visible { get; set; }

        public int FileSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [Column("filedata_thumb", TypeName = "MEDIUMBLOB")]
        public byte[] ThumbnailFileData { get; set; }

        [Column("width_thumb")]
        public int ThumbnailWidth { get; set; }

        [Column("height_thumb")]
        public int ThumbnailHeight { get; set; }

        [NotMapped]
        public DateTime CreatedTime {
            get => DateTimeExtensions.ToDateTime(CreatedTimeRaw);
            set => CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

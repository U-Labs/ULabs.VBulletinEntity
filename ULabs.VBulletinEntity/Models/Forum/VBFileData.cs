using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ULabs.VBulletinEntity.Shared.Tools;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("filedata")]
    public class VBFileData {
        [Column("filedataid")]
        public int Id { get; set; }
        public int UserId { get; set; }

        [Column("dateline")]
        public int CreatedTimeRaw { get; set; }

        [Column("thumbnail_dateline")]
        public int ThumbnailCreatedTimeRaw { get; set; }

        [Column(TypeName = "MEDIUMBLOB")]
        public byte[] FileData { get; set; }

        public int FileSize { get; set; }

        [MaxLength(32)]
        public string FileHash { get; set; }

        [Column(TypeName = "MEDIUMBLOB")]
        public byte[] Thumbnail { get; set; }

        [Column("thumbnail_filesize")]
        public int ThumbnailFileSize { get; set; }

        [MaxLength(32)]
        public string Extension { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        [Column("thumbnail_width")]
        public int ThumbnailWidth { get; set; }

        [Column("thumbnail_height")]
        public int ThumbnailHeight { get; set; }

        public int RefCount { get; set; }

        [NotMapped]
        public DateTime CreatedTime {
            get { return CreatedTimeRaw.ToDateTime(); }
            set { CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        [NotMapped]
        public DateTime ThumbnailCreatedTime {
            get { return ThumbnailCreatedTimeRaw.ToDateTime(); }
            set { ThumbnailCreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }
    }
}

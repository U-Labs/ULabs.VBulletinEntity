using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using ULabs.VBulletinEntity.Models.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("attachment")]
    public class VBAttachment {
        [Column("attachmentid"), Key]
        public int Id { get; set; }

        public int ContentTypeId { get; set; }

        public int ContentId { get; set; }

        public int UserId { get; set; }
        public VBUser User { get; set; }

        [Column("dateline")]
        public int CreatedTimeRaw { get; set; }

        [NotMapped]
        public DateTime CreatedTime {
            get { return CreatedTimeRaw.ToDateTime(); }
            set { CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        public int FileDataId { get; set; }
        public VBFileData FileData { get; set; }

        [Column("state")]
        public string StateRaw { get; set; }

        [Column("counter")]
        public int DownloadsCount { get; set; }

        public string PostHash { get; set; }

        public string FileName { get; set; }

        public string Caption { get; set; }

        public int ReportThreadId { get; set; }

        public string Settings { get; set; }

        public int DisplayOrder { get; set; }

        [NotMapped]
        public string FilePath {
            get {
                // Mostly the attachment userid equals to filedata userid. But sometimes not, which result in non existing files. To be sure we use FileData
                if(FileData==null) {
                    throw new Exception($"Attachment #{Id}: FileData must be included in order to correctly determine the attachment path");
                }
                char[] userIdSegments = FileData.UserId.ToString().ToCharArray();
                string path = string.Join("/", userIdSegments);
                return $"{path}/{FileDataId}.attach";
            }
        }
    }
}

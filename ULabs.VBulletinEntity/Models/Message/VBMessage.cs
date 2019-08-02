using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Message {
    [Table("pm")]
    public class VBMessage {
        [Column("pmid"), Key]
        public int Id { get; set; }

        [Column("pmtextid")]
        public int TextId { get; set; }
        public VBMessageText Text { get; set; }

        [Column("userid")]
        public int AuthorId { get; set; }

        public int FolderId { get; set; }

        // ToDo: Check what messageread means (We have 0/1/2)
        [Column("messageread")]
        public int MessageReadRaw { get; set; }

        public int ParentPmId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Message {
    [Table("pm")]
    public class VBMessage {
        [Column("pmid")]
        public int Id { get; set; }

        [Column("pmtextid")]
        public int TextId { get; set; }
        public VBMessageText Text { get; set; }

        [Column("userid")]
        public int AuthorId { get; set; }

        public int FolderId { get; set; }

        [Column("messageread")]
        public int MessageReadRaw { get; set; }

        [Column("parentpmid")]
        public int ParentMessageId { get; set; }
        public VBMessage ParentMessage { get; set; }

        public VBMessageReadState MessageRead {
            get => (VBMessageReadState)Enum.Parse(typeof(VBMessageReadState), MessageReadRaw.ToString());
            set => MessageReadRaw = (int)value;
        }
    }

    public enum VBMessageReadState {
        Unread = 0,
        Read = 1,
        Answered = 2
    }
}

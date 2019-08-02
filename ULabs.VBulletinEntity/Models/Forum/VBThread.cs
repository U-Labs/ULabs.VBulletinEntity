using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("thread")]
    [JsonObject(IsReference = true)]
    public class VBThread {
        [Column("threadid")]
        public int Id { get; set; }

        [Column("title"), MaxLength(250)]
        public string Title { get; set; }

        [Column("prefixid"), MaxLength(25)]
        // ToDo: Check prefix relation
        public string PrefixId { get; set; }

        [Column("firstpostid")]
        public int FirstPostId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBPost FirstPost { get; set; }

        [Column("lastpostid")]
        public int LastPostId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBPost LastPost { get; set; }

        [Column("lastpost")]
        public int LastPostTimeRaw { get; set; }

        [NotMapped]
        public DateTime LastPostTime {
            get { return LastPostTimeRaw.ToDateTime(); }
            set { LastPostTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }
    }
}

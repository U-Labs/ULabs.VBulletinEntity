using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("thread")]
    [JsonObject(IsReference = true)]
    public class VBThread {
        [Column("threadid")]
        public int Id { get; set; }

        [Column("title"), MaxLength(250)]
        public string Title { get; set; }
    }
}

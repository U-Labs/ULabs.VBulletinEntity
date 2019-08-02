using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("post")]
    [JsonObject(IsReference = true)]
    public class VBPost {
        [Column("postid")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

    }
}

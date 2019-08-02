using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Config {
    public class VBShowThreadSettings {
        [Column("maxposts")]
        public int PostsPerPage { get; set; }
    }
}

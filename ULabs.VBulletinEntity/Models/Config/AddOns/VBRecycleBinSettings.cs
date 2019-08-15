using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Config.AddOns {
    /// <summary>
    /// Settings for VB Addon "Move threads To Recycle Bin" ~> https://www.vbulletin.org/forum/showthread.php?t=106774
    /// </summary>
    public class VBRecycleBinSettings {
        [Column("recycle_forum")]
        public int RecycleForumId { get; set; }
    }
}

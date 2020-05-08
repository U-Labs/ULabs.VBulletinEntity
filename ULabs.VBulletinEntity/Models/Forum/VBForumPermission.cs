using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ULabs.VBulletinEntity.Shared.Permission;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("forumpermission")]
    public class VBForumPermission {
        [Column("forumpermissionid")]
        public int Id { get; set; }

        public int ForumId { get; set; }
        //public VBForum Forum { get; set; }

        public int UserGroupId { get; set; }

        [Column("forumpermissions")]
        public VBForumFlags Flag { get; set; }

        // EF
        public VBForumPermission() { }

        public VBForumPermission(int forumPermissionRaw) {
            Flag = (VBForumFlags)forumPermissionRaw;
        }
    }
}

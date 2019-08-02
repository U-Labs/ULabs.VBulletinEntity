using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ULabs.VBulletinEntity.Models.Permission;

namespace ULabs.VBulletinEntity.Models.User {
    [Table("usergroup")]
    public class VBUserGroup {
        [Column("usergroupid")]
        public int Id { get; set; }

        [Column("title"), MaxLength(100)]
        public string Title { get; set; }

        [Column("description"), MaxLength(500)]
        public string Description { get; set; }

        [Column("usertitle"), MaxLength(100)]
        public string UserTitle { get; set; }

        [Column("passwordexpires")]
        public int PasswordExpiresDays { get; set; }

        [Column("passwordhistory")]
        public int PasswordHistory { get; set; }

        [Column("pmquota")]
        public int PmQuota { get; set; }

        [Column("pmsendmax")]
        public int PmSendLimit { get; set; }

        [Column("opentag"), MaxLength(100)]
        public string HtmlOpenTag { get; set; }

        [Column("closetag"), MaxLength(100)]
        public string HtmlCloseTag { get; set; }

        [Column("canoverride", TypeName = "SMALLINT")]
        public bool CanOverride { get; set; }

        [Column("ispublicgroup", TypeName = "SMALLINT")]
        public bool IsPublicGroup { get; set; }

        // ToDo: Permissions (see Overview-Project for implementation that can be re-used)
        [Column("forumpermissions")]
        public VBForumFlags ForumPermissions { get; set; }

        // First test case for directly using flags without custom int attribute for mapping
        // ToDo: Merge ForumPermissionsRaw with ForumPermissions if it works 
        [Column("adminpermissions")]
        public VBAdminFlags AdminPermissions { get; set; }

        [Column("attachlimit")]
        public int AttachmentLimit { get; set; }

        [Column("avatarmaxwidth")]
        public int AvatarMaxWidth { get; set; }

        [Column("avatarmaxheight")]
        public int AvatarMaxHeight { get; set; }

        [Column("avatarmaxsize")]
        public int AvatarMaxSize { get; set; }

        // ToDo: Profile pic and signature 
    }
}

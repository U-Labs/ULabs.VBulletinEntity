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

        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string UserTitle { get; set; }

        [Column("passwordexpires")]
        public int PasswordExpiresDays { get; set; }

        public int PasswordHistory { get; set; }
        public int PmQuota { get; set; }

        [Column("pmsendmax")]
        public int PmSendLimit { get; set; }

        [Column("opentag"), MaxLength(100)]
        public string HtmlOpenTag { get; set; }

        [Column("closetag"), MaxLength(100)]
        public string HtmlCloseTag { get; set; }

        [Column(TypeName = "SMALLINT")]
        public bool CanOverride { get; set; }

        [Column(TypeName = "SMALLINT")]
        public bool IsPublicGroup { get; set; }

        // ToDo: Permissions (see Overview-Project for implementation that can be re-used)
        // Missing: pmpermissions, calendarpermissions, wolpermissions, genericpermissions, genericpermissions2, genericoptions, signaturepermissions, visitormessagepermissions
        public VBForumFlags ForumPermissions { get; set; }

        // First test case for directly using flags without custom int attribute for mapping
        public VBAdminFlags AdminPermissions { get; set; }

        [Column("attachlimit")]
        public int AttachmentLimit { get; set; }
        public int AvatarMaxWidth { get; set; }
        public int AvatarMaxHeight { get; set; }
        public int AvatarMaxSize { get; set; }
        public int ProfilePicMaxWidth { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Models.Permission;

namespace ULabs.VBulletinEntity.LightModels.User {
    public class VBLightUserGroup {
        public int Id { get; set; }
        public string OpenTag { get; set; }
        public string CloseTag { get; set; }
        // ToDo: Rename to Title
        public string UserTitle { get; set; }
        public VBAdminFlags AdminPermissions { get; set; }
    }
}

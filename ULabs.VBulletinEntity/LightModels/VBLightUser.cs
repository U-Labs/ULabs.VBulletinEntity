﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels {
    public class VBLightUser {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserTitle { get; set; }
        public int PrimaryUserGroupId { get; set; }
        public int AvatarRevision { get; set; }
    }
}
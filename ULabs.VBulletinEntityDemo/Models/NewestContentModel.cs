using System;
using System.Collections.Generic;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Models.User;

namespace ULabs.VBulletinEntityDemo.Models {
    public class NewestContentModel {
        public List<VBThread> Threads { get; set; }
        public List<VBPost> Posts { get; set; }
        public List<VBUser> Users { get; set; }
    }
}
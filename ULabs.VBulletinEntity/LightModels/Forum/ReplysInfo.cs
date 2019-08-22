using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels.Forum {
    public class ReplysInfo {
        public int TotalPages { get; set; }
        public int ReplysPerPage { get; set; }
        public List<int> PostIds { get; set; }
    }
}

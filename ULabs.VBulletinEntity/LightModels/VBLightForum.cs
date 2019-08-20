using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels {
    public class VBLightForum {
        public int ForumId { get; set; }
        public string Title { get; set; }
        public int ParentId { get; set; }
        public string ParentIdsRaw { get; set; }
        public List<int> ParentIds {
            get => ParentIdsRaw?.Split(',')
                .Select(int.Parse)
                .ToList();
        }
    }
}

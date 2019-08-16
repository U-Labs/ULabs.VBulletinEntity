using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels {
    public class VBLightCategoryWithChilds {
        public int ForumId { get; set; }
        public string ChildsRaw { get; set; }
        public List<int> Childs {
            get => ChildsRaw.Split(',')
                .Select(int.Parse)
                .ToList();
        }
    }
}

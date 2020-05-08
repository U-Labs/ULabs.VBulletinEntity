using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.LightVBulletinEntity.LightModels {
    public class PageContentInfo {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ReplysPerPage { get; set; }
        public List<int> ContentIds { get; set; }
        public PageContentInfo(int currentPage, int replysPerPage) {
            CurrentPage = currentPage;
            ReplysPerPage = replysPerPage;
        }
    }
}

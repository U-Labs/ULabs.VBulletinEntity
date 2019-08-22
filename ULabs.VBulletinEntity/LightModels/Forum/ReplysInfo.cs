using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels.Forum {
    public class ReplysInfo {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ReplysPerPage { get; set; }
        public List<int> PostIds { get; set; }
        public ReplysInfo(int currentPage, int replysPerPage) {
            CurrentPage = currentPage;
            ReplysPerPage = replysPerPage;
        }
    }
}

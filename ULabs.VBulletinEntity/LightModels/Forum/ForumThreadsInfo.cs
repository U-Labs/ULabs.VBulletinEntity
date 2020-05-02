using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels.Forum {
    public class ForumThreadsInfo {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ThreadsPerPage { get; set; }
        public List<int> ThreadIds { get; set; }
        public ForumThreadsInfo(int currentPage, int threadsPerPage) {
            CurrentPage = currentPage;
            ThreadsPerPage = threadsPerPage;
        }
    }
}

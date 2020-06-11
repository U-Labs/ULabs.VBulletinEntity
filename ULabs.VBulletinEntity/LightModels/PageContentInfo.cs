using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels {
    public class PageContentInfo {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int RowsPerPage { get; set; }
        public List<int> ContentIds { get; set; }
        public PageContentInfo(int currentPage, int rowsPerPage) {
            CurrentPage = currentPage;
            RowsPerPage = rowsPerPage;
        }
    }
}

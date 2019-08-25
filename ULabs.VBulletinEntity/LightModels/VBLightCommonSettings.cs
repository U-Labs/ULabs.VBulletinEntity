using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels {
    public class VBLightCommonSettings {
        public string BaseUrl { get; set; }
        public int PostsPerPage { get; set; }
        /// <summary>
        /// Specified forum id for trash by Addon "Move Threads to Recycle Bin"
        /// </summary>
        public int RecycleBinForumId { get; set; }

        public VBLightCommonSettings(Dictionary<string,string> sqlData) {
            BaseUrl = sqlData["bburl"];
            PostsPerPage = int.Parse(sqlData["maxposts"]);

            // Checking for the presence of Addon settings make the class useable for users without having this addon installed
            if(sqlData.ContainsKey("recycle_forum")) {
                RecycleBinForumId = int.Parse(sqlData["recycle_forum"]);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using ULabs.LightVBulletinEntity.LightModels.User;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.LightVBulletinEntity.LightModels.Forum {
    public class VBLightThread {
        public int Id { get; set; }
        public string Title { get; set; }
        public int CreatedTimeRaw { get; set; }
        public DateTime CreatedTime {
            get => CreatedTimeRaw.ToDateTime();
            set => CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        public int LastPostTimeRaw { get; set; }
        public VBLightUser LastPoster { get; set; }
        // This entity has FirstPost and FirstPostProperty since we fetch the first post in an extra query instead of one single large join (would be too large and complex)
        public int FirstPostId { get; set; }
        // Fits better here than fetching it from the results. Especially on paging it's not possible any more to fetch it from the replys since up from page 2, the first post isn't included
        // any more in the replys list. We can also cleaner divide between first post and replys with this attribute.
        public VBLightPost FirstPost { get; set; }
        public int LastPostId { get; set; }
        public int AuthorUserId { get; set; }
        public VBLightForum Forum { get; set; }
        public int ReplysCount { get; set; }
        public int DeletedReplysCount { get; set; }
        public bool IsOpen { get; set; }
        public bool IsVisible { get; set; }

        public DateTime LastPostTime {
            get => LastPostTimeRaw.ToDateTime();
            set => LastPostTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
        public string HtmlDecodedTitle {
            get => HttpUtility.HtmlDecode(Title);
        }

        /// <summary>
        /// Generates the thread URL part from VBSEO by pattern {ForumId}-{ThreadTitle}
        /// </summary>
        public string SeoUrlPart {
            get => $"{ContentTools.SeoTitle(Title)}-{Id}";
        }
    }
}

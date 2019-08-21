using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using ULabs.VBulletinEntity.LightModels.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightModels.Forum {
    public class VBLightThread {
        public string Title { get; set; }
        public int ThreadId { get; set; }
        public int LastPostTimeRaw { get; set; }
        public string LastPosterName { get; set; }
        public VBLightUser LastPoster { get; set; }
        public int LastPosterUserId { get; set; }
        public int FirstPostId { get; set; }
        public int LastPostId { get; set; }
        public int ForumId { get; set; }
        public VBLightForum Forum { get; set; }
        public int ReplysCount { get; set; }
        public int DeletedReplysCount { get; set; }
        public bool IsOpen { get; set; }

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
            get => $"{ContentTools.SeoTitle(Title)}-{ThreadId}";
        }
    }
}

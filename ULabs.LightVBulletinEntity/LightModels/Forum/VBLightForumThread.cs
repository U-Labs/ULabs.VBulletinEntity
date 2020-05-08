using System;
using System.Collections.Generic;
using System.Text;
using ULabs.LightVBulletinEntity.Tools;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.LightVBulletinEntity.LightModels.Forum {
    public class VBLightForumThread {
        public int Id { get; set; }
        // For category boxes URL generation, where we get threads from differnt forum id
        public int ForumId { get; set; }
        public string Title { get; set; }
        public bool Open { get; set; }
        public int ReplysCount { get; set; }
        public string AuthorUserName { get; set; }
        public int AuthorUserId { get; set; }
        public string LastPosterUserName { get; set; }
        public int LastPosterUserId { get; set; }
        public int LastPostTimeRaw { get; set; }
        public int ViewsCount { get; set; }
        public int CreatedTimeRaw { get; set; }
        public DateTime CreatedTime {
            get => CreatedTimeRaw.ToDateTime();
            set => CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
        public DateTime LastPostTime {
            get => LastPostTimeRaw.ToDateTime();
            set => LastPostTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }
}

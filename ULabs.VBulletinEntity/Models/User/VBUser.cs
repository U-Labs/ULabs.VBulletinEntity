using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.User {
    [Table("user")]
    [JsonObject(IsReference = true)]
    public class VBUser {
        [Column("userid")]
        public int Id { get; set; }

        public int UserGroupId { get; set; }
        public VBUserGroup UserGroup { get; set; }

        [Column("membergroupids")]
        public string MemberGroupIdsRaw { get; set; }

        [NotMapped]
        public List<int> MemberGroupIds {
            get {
                if (string.IsNullOrEmpty(MemberGroupIdsRaw)) {
                    return new List<int>();
                }
                return MemberGroupIdsRaw.Split(',')
                    .ToList()
                    .Select(int.Parse)
                    .ToList();
            }
            set {
                MemberGroupIdsRaw = string.Join(",", value);
            }
        }

        public int? DisplayGroupId { get; set; }
        public VBUserGroup DisplayGroup { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int LastPostId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBPost LastPost { get; set; }

        [Column("posts")]
        public int PostsCount { get; set; }

        [Column("joindate")]
        public int JoinDateRaw { get; set; }

        [NotMapped]
        public DateTime JoinDate {
            get { return DateTimeExtensions.ToDateTime(JoinDateRaw); }
            set { JoinDateRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        public int AvatarRevision { get; set; }

        public VBCustomAvatar CustomAvatar { get; set; }

        [NotMapped]
        public string AvatarFileName {
            get {
                if (CustomAvatar == null)
                    return "";
                return $"avatar{Id}_{AvatarRevision}.gif";
            }
        }

        public string GetAvatarUrl(string forumBaseUrl) {
            if (CustomAvatar == null) {
                // ToDo: VB has not setting for the default Avatar. We should specify this in custom settings somewhere
                return "https://u-img.net/img/4037Ld.png";
            }
            return $"{forumBaseUrl}/customavatars/{AvatarFileName}";
        }

        [Column("lastvisit")]
        public int LastVisitTimeRaw { get; set; }

        [NotMapped]
        public DateTime LastVisitTime {
            get { return DateTimeExtensions.ToDateTime(LastVisitTimeRaw); }
            set { LastVisitTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        [Column("lastactivity")]
        public int LastActivityTimeRaw { get; set; }

        [NotMapped]
        public DateTime LastActivityTime {
            get { return DateTimeExtensions.ToDateTime(LastActivityTimeRaw); }
            set { LastActivityTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        [Column("lastpost")]
        public int LastPostTimeRaw { get; set; }

        [NotMapped]
        public DateTime LastPostTime {
            get { return DateTimeExtensions.ToDateTime(LastPostTimeRaw); }
            set { LastPostTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }
        // ToDo: Handle self referencing loops doesn't full work yet. On the session we get: 
        // JsonSerializationException: Self referencing loop detected with type 'ULabs.VBulletinEntity.Models.Forum.VBPost'. Path 'User.LastPost.Thread.Replys'.
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public List<VBPost> Posts { get; set; }
    }
}


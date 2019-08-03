using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
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
        public int? DisplayGroupId { get; set; }
        public VBUserGroup DisplayGroup { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        [Column(TypeName = "DATE")]
        public DateTime PasswordDate { get; set; }
        public string Email { get; set; }
        public int StyleId { get; set; }

        [MaxLength(50)]
        public string ParentEmail { get; set; }

        [MaxLength(100)]
        public string Homepage { get; set; }

        [MaxLength(20)]
        public string Icq { get; set; }
        [MaxLength(20)]
        public string Aim { get; set; }
        [MaxLength(20)]
        public string Yahoo { get; set; }
        [MaxLength(20)]
        public string MSN { get; set; }
        [MaxLength(20)]
        public string Skype { get; set; }

        // ToDo: Generate enum (Value range seems to be from 0 to 2)
        public int ShowVBCode { get; set; }
        public int ShowBirthday { get; set; }

        [MaxLength(250)]
        public string UserTitle { get; set; }
        // ToDo: Generate enum
        public int CustomTitle { get; set; }

        [Column("joindate")]
        public int JoinDateRaw { get; set; }

        // ToDo: Generate enum
        public int Daysprune { get; set; }

        [Column("lastvisit")]
        public int LastVisitTimeRaw { get; set; }

        [Column("lastactivity")]
        public int LastActivityTimeRaw { get; set; }

        [Column("lastpost")]
        public int LastPostTimeRaw { get; set; }
        public int LastPostId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBPost LastPost { get; set; }

        [Column("posts")]
        public int PostsCount { get; set; }
        public int Reputation { get; set; }
        public int ReputationLevelId { get; set; }
        [Column("timezoneoffset"), MaxLength(4)]
        public string TimezoneOffsetRaw { get; set; }
        // ToDo: Create enum
        public int PmPopup { get; set; }

        public int AvatarId { get; set; }
        public int AvatarRevision { get; set; }
        public int ProfilePicRevision { get; set; }
        public int SigPicRevision { get; set; }
        // ToDo: Create enum
        public int Options { get; set; }

        // birthday_search not implemented here since it contains the same data than Birthday but in another format
        [Column("birthday"), DataType(DataType.Date)]
        public string BirthdayRaw { get; set; }
        // Seems like the post per page settings - ToDo: Verify and document
        public int MaxPosts { get; set; }

        public int StartOfWeek { get; set; }

        [Column(TypeName = "VARCHAR(45)")]
        public string IPAddress { get; set; }
        public VBCustomAvatar CustomAvatar { get; set; }

        public int ReferrerId { get; set; }
        public VBUser Referrer { get; set; }
        // ToDo: Check FK
        public int LanguageId { get; set; }

        [Column("emailstamp")]
        public int EmailStampRaw { get; set; }
        // ToDo: Check what this is used for, seems a boolean calue
        public int ThreadedMode { get; set; }
        // ToDo: Check - Seems also some kind of boolean but with -1 values
        public int AutoSubscribe { get; set; }

        [Column("pmtotal")]
        public int PmTotalCount { get; set; }

        [Column("pmunread")]
        public int PmUnreadCount { get; set; }

        [Column("salt"), MaxLength(50)]
        public string PasswordSalt { get; set; }

        [Column("ipoints")]
        public int InfractionPoints { get; set; }

        [Column("infractions")]
        public int InfractionsCount { get; set; }

        [Column("warnings")]
        public int WarningsCount { get; set; }

        // ToDo: Check - were both empty for all U-Labs users
        [MaxLength(255)]
        public string InfractionGroupIds { get; set; }
        public int InfractionGroupId { get; set; }

        // ToDo: Create enum
        public int AdminOptions { get; set; }

        [Column("profilevisits")]
        public int ProfileVisitsCount { get; set; }
        public int FriendCount { get; set; }

        [Column("friendreqcount")]
        public int FriendRequestsCount { get; set; }

        // ToDo: Check what VM is
        public int VmUnreadCount { get; set; }
        public int VmModeratedCount { get; set; }

        [Column("socgroupinvitecount")]
        public int SocialGroupInviteCount { get; set; }
        [Column("socgroupreqcount")]
        public int SocialGroupRequestsCount { get; set; }

        // ToDo: Check what PC/GM is
        public int PcUnreadCount { get; set; }
        public int PcModeratedCount { get; set; }
        public int GmModeratedCount { get; set; }

        // ToDo: Handle self referencing loops doesn't full work yet. On the session we get: 
        // JsonSerializationException: Self referencing loop detected with type 'ULabs.VBulletinEntity.Models.Forum.VBPost'. Path 'User.LastPost.Thread.Replys'.
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public List<VBPost> Posts { get; set; }

        #region NotMapped 
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
        [NotMapped]
        public DateTime Birthday {
            get => DateTime.Parse(BirthdayRaw);
            set {
                BirthdayRaw = value.ToString("MM-dd-yy");
            }
        }
        [NotMapped]
        public int TimezoneOffset {
            get => int.Parse(TimezoneOffsetRaw);
            set => TimezoneOffsetRaw = value.ToString();
        }

        [NotMapped]
        public DateTime JoinDate {
            get => DateTimeExtensions.ToDateTime(JoinDateRaw);
            set => JoinDateRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        [NotMapped]
        public DateTime EmailStamp {
            get => DateTimeExtensions.ToDateTime(EmailStampRaw);
            set => EmailStampRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        [NotMapped]
        public string AvatarFileName {
            get {
                if (CustomAvatar == null)
                    return "";
                return $"avatar{Id}_{AvatarRevision}.gif";
            }
        }

        [NotMapped]
        public DateTime LastVisitTime {
            get { return DateTimeExtensions.ToDateTime(LastVisitTimeRaw); }
            set { LastVisitTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        [NotMapped]
        public DateTime LastActivityTime {
            get { return DateTimeExtensions.ToDateTime(LastActivityTimeRaw); }
            set { LastActivityTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        [NotMapped]
        public DateTime LastPostTime {
            get { return DateTimeExtensions.ToDateTime(LastPostTimeRaw); }
            set { LastPostTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }
        public string GetAvatarUrl(string forumBaseUrl) {
            if (CustomAvatar == null) {
                // ToDo: VB has not setting for the default Avatar. We should specify this in custom settings somewhere
                return "https://u-img.net/img/4037Ld.png";
            }
            return $"{forumBaseUrl}/customavatars/{AvatarFileName}";
        }
        #endregion
    }
}


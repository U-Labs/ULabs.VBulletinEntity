using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.User {
    [Table("session")]
    [JsonObject(IsReference = true)]
    public class VBSession {
        [Key, MaxLength(32)]
        public string SessionHash { get; set; }

        public int? UserId { get; set; }

        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBUser User { get; set; }

        [MaxLength(45)]
        public string Host { get; set; }

        public string IdHash { get; set; }

        [Column("lastactivity")]
        public int LastActivityRaw { get; set; }

        [NotMapped]
        public DateTime LastActivity {
            get => DateTimeExtensions.ToDateTime(LastActivityRaw);
            set => LastActivityRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        [MaxLength(255)]
        public string Location { get; set; }

        [MaxLength(255)]
        public string UserAgent { get; set; }

        public int StyleId { get; set; }
        public int LanguageId { get; set; }

        /// <summary>
        /// 0 is a guest session, 1 and 2 means logged in: 1 = First page view after login, 2 = User has viewed more than one page after login (see includes/functions.php:7558)
        /// </summary>
        [Column("loggedin")]
        public int LoggedInRaw { get; set; }

        [Column("inforum")]
        public int InForumId { get; set; }
        public VBForum InForum{ get; set; }

        [Column("inthread")]
        public int InThreadId { get; set; }
        public VBThread InThread { get; set; }

        [Column("incalendar")]
        public int InCalendarId { get; set; }

        /// <summary>
        /// BadLocation = 3 when viewing non existing thread/post
        /// </summary>
        public int BadLocation { get; set; }
        public int Bypass { get; set; }
        public int ProfileUpdate { get; set; }
        public int ApiClientId { get; set; }

        [MaxLength(32)]
        public string ApiAccessToken { get; set; }

        // vB uses TINYINT but since only boolean values are expected, we can set the SQL type to bit 
        [Column(TypeName = "bit")]
        public bool IsBot { get; set; }

        [NotMapped]
        public bool LoggedIn {
            get { return LoggedInRaw == 2 || LoggedInRaw == 1; }
        }
    }
}

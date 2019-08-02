using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("forum")]
    public class VBForum {
        [Column("forumid")]
        public int Id { get; set; }

        public string Title { get; set; }

        [NotMapped]
        public string SeoTitle {
            get => ContentTools.SeoTitle(Title);
        }

        public int LastThreadId { get; set; }

        [Column("lastthread")]
        public string LastThreadTitle { get; set; }

        public int LastPostId { get; set; }

        [Column("lastposter")]
        public string LastPosterUsername { get; set; }

        [Column("replycount")]
        public int PostCount { get; set; }

        public int ThreadCount { get; set; }

        [Column("lastpost")]
        public int LastPostDateRaw { get; set; }

        public List<VBForumPermission> Permissions { get; set; }

        [Column("childlist")]
        public string ChildListRaw { get; set; }

        [NotMapped]
        public List<int> ChildList {
            get {
                return ChildListRaw.Split(new string[] { "," }, StringSplitOptions.None)
                  .ToList()
                  .Select(int.Parse)
                  .Where(id => id != -1)
                  .ToList();
            }
            set {
                // VB always set -1 at the end of the childlist
                if (!value.Contains(-1)) {
                    value.Add(-1);
                }
                ChildListRaw = string.Join(",", value);
            }
        }

        // ToDo: Some attributes missing

        public int? ParentId { get; set; }
        public VBForum Parent { get; set; }

        [NotMapped]
        public DateTime LastPostDate {
            get => LastPostDateRaw.ToDateTime();
            set => LastPostDateRaw = value.ToUnixTimestampAsInt();
        }

        [NotMapped]
        public string HtmlDecodedTitle {
            get {
                var decoded = HttpUtility.HtmlDecode(Title);
                return decoded;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using ULabs.VBulletinEntity.Models.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("forum")]
    public class VBForum {
        [Column("forumid")]
        public int Id { get; set; }

        public int StyleId { get; set; }
        public string Title { get; set; }

        [Column("title_clean")]
        public string TitleClean { get; set; }

        // ToDo: Implement enum
        [Column("options")]
        public int OptionsRaw { get; set; }

        public bool ShowPrivate { get; set; }

        // ToDo: Implement enum
        [Column("displayorder")]
        public int DisplayOrderRaw { get; set; }

        [Column("replycount")]
        public int PostCount { get; set; }

        [Column("lastpost")]
        public int LastPostDateRaw { get; set; }

        [Column("lastposter")]
        public string LastPosterName { get; set; }
        public int LastPostId { get; set; }
        public VBPost LastPost { get; set; }

        [Column("lastthread")]
        public string LastThreadTitle { get; set; }
        public int LastThreadId { get; set; }
        public int LastIconId { get; set; }

        [MaxLength(250)]
        public string LastPrefixId { get; set; }

        [Column("threadcount")]
        public int ThreadsCount { get; set; }
        public int Daysprune { get; set; }
        public string NewPostEmail { get; set; }
        public string NewThreadEmail { get; set; }
        public int? ParentId { get; set; }
        public VBForum Parent { get; set; }

        [Column("parentlist"), MaxLength(250)]
        public string ParentListRaw { get; set; }

        [MaxLength(50)]
        public string Password { get; set; }

        [MaxLength(200)]
        public string Link { get; set; }

        [Column("childlist")]
        public string ChildListRaw { get; set; }

        [MaxLength(50)]
        public string DefaultSortField { get; set; }

        [Column("defaultsortorder")]
        public string DefaultSortOrderRaw { get; set; }

        [MaxLength(100)]
        public string ImagePrefix { get; set; }

        [Column("lastposterid")]
        public int LastPostAuthorId { get; set; }
        public VBUser LastPostAuthor { get; set; }
        public List<VBForumPermission> Permissions { get; set; }

        [NotMapped]
        public VBForumDefaultSortOrder DefaultSortOrder {
            get {
                // Convert the lowercase "desc" from vBulletin to uppercase "Desc" that matches our enum value
                if(string.IsNullOrEmpty(DefaultSortOrderRaw) || DefaultSortOrderRaw.Length == 0) {
                    return default;
                }
                string upperDefaultSortOrder = Char.ToUpperInvariant(DefaultSortOrderRaw[0]) + DefaultSortOrderRaw.Substring(1, DefaultSortOrderRaw.Length - 1);
                return (VBForumDefaultSortOrder)Enum.Parse(typeof(VBForumDefaultSortOrder), upperDefaultSortOrder);
            }
            set => DefaultSortOrderRaw = value.ToString();
        }

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

        [NotMapped]
        public string SeoTitle {
            get => ContentTools.SeoTitle(Title);
        }

        [NotMapped]
        public DateTime LastPostDate {
            get => LastPostDateRaw.ToDateTime();
            set => LastPostDateRaw = value.ToUnixTimestampAsInt();
        }

        [NotMapped]
        public string HtmlDecodedTitle {
            get => HttpUtility.HtmlDecode(Title);
        }
    }

    public enum VBForumDefaultSortOrder {
        Asc,
        Desc
    }
}

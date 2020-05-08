using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;
using ULabs.VBulletinEntity.Models.User;
using ULabs.VBulletinEntity.Tools;
using System.Web;
using System.Linq;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("thread")]
    [JsonObject(IsReference = true)]
    public class VBThread {
        [Column("threadid")]
        public int Id { get; set; }

        [MaxLength(250)]
        public string Title { get; set; }

        [MaxLength(25)]
        public string PrefixId { get; set; }

        public int FirstPostId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBPost FirstPost { get; set; }

        public int LastPostId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBPost LastPost { get; set; }

        [Column("lastpost")]
        public int LastPostTimeRaw { get; set; }

        public int ForumId { get; set; }
        public VBForum Forum { get; set; }

        // Nullable with default Value 0 in Context to make vB behavior conform to EF
        public int? PollId { get; set; }
        public VBPoll Poll { get; set; }

        [Column("open")]
        public bool IsOpen { get; set; }

        [Column("replycount")]
        public int ReplysCount { get; set; }

        [Column("hiddencount")]
        public int HiddenPostsCount { get; set; }

        [Column("deletedcount")]
        public int DeletedPostsCount { get; set; }

        [Column("postusername"), MaxLength(100)]
        public string AuthorName { get; set; }

        [Column("postuserid")]
        public int AuthorId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBUser Author { get; set; }

        [Column("lastposter"), MaxLength(100)]
        public string LastPostAuthorName { get; set; }

        [Column("dateline")]
        public int CreatedTimeRaw { get; set; }

        public int Views { get; set; }

        public int IconId { get; set; }

        [MaxLength(250)]
        public string Notes { get; set; }

        [Column("visible")]
        public bool IsVisible { get; set; }

        [Column("sticky")]
        public bool IsSticky { get; set; }

        [Column("votenum")]
        public int VotesCount { get; set; }

        [Column("votetotal")]
        public int VotesScore { get; set; }

        [Column("attach")]
        public int AttachmentsCount { get; set; }

        [Column("similar"), MaxLength(55)]
        public string SimilarThreadIdsRaw { get; set; }
        public string Taglist { get; set; }

        [Column("lastposterid")]
        public int LastPostAuthorId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBUser LastPostAuthor { get; set; }

        [Column("keywords")]
        public string KeywordsRaw { get; set; }
        public int PosterCount { get; set; }

        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public List<VBPost> Replys { get; set; }

        public VBThread() { }

        public VBThread(int lastPostAuthorId, string lastPostAuthorName, int firstPostId, int lastPostId, int forumId, int authorId, string authorName, DateTime createdTime, DateTime lastPostTime,
            string title, bool isOpen = true, bool isVisible = true, int views = 0, string notes = "", string prefixId = "") {
            LastPostAuthorId = lastPostAuthorId;
            LastPostAuthorName = lastPostAuthorName;
            FirstPostId = firstPostId;
            LastPostId = lastPostId;
            ForumId = forumId;
            AuthorId = authorId;
            AuthorName = authorName;
            CreatedTime = createdTime.ForceUtc();
            LastPostTime = lastPostTime.ForceUtc();
            Title = title;
            IsOpen = isOpen;
            IsVisible = isVisible;
            Views = views;
            Notes = notes;
            PrefixId = prefixId;
            // Cannot be null. If we don't have similar ones it seems that VB also set it to an empty string
            SimilarThreadIdsRaw = string.Empty;
        }

        [NotMapped]
        public List<int> SimilarThreadIds {
            get {
                if (string.IsNullOrEmpty(SimilarThreadIdsRaw)) {
                    return new List<int>();
                }
                return SimilarThreadIdsRaw.Split(',')
                    .Select(int.Parse)
                    .ToList();
            }
            set => SimilarThreadIdsRaw = string.Join(",", value);
        }

        [NotMapped]
        public List<string> Keywords {
            get {
                if (string.IsNullOrEmpty(KeywordsRaw)) {
                    return new List<string>();
                }
                return KeywordsRaw.Split(',').ToList();
            }
            set => KeywordsRaw = string.Join(",", value);
        }

        [NotMapped]
        public string HtmlDecodedTitle {
            get {
                return HttpUtility.HtmlDecode(Title);
            }
        }

        [NotMapped]
        public string SeoTitle {
            get => ContentTools.SeoTitle(Title);
        }

        [NotMapped]
        public DateTime LastPostTime {
            get { return LastPostTimeRaw.ToDateTime(); }
            set { LastPostTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        [NotMapped]
        public DateTime CreatedTime {
            get { return CreatedTimeRaw.ToDateTime(); }
            set { CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }
    }
}

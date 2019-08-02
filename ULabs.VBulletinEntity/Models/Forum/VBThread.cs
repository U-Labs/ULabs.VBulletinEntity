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

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("thread")]
    [JsonObject(IsReference = true)]
    public class VBThread {
        [Column("threadid")]
        public int Id { get; set; }

        [Column("title"), MaxLength(250)]
        public string Title { get; set; }

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

        [Column("prefixid"), MaxLength(25)]
        public string PrefixId { get; set; }

        [Column("firstpostid")]
        public int FirstPostId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBPost FirstPost { get; set; }

        [Column("lastpostid")]
        public int LastPostId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBPost LastPost { get; set; }

        [Column("lastpost")]
        public int LastPostTimeRaw { get; set; }

        [NotMapped]
        public DateTime LastPostTime {
            get { return LastPostTimeRaw.ToDateTime(); }
            set { LastPostTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        public int ForumId { get; set; }
        public VBForum Forum { get; set; }

        // Nullable with default Value 0 in Context to make vB behavior conform to EF
        public int? PollId { get; set; }
        public VBPoll Poll { get; set; }

        [Column("open")]
        public bool IsOpen { get; set; }

        [Column("replycount")]
        public int ReplyPostsCount { get; set; }

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

        [NotMapped]
        public DateTime CreatedTime {
            get { return CreatedTimeRaw.ToDateTime(); }
            set { CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        public int Views { get; set; }

        public int IconId { get; set; }

        [Column("notes"), MaxLength(250)]
        public string Notes { get; set; }

        [Column("visible")]
        public bool IsVisible { get; set; }

        [Column("sticky")]
        public bool IsSticky { get; set; }

        [Column("votenum")]
        public int VotesCount { get; set; }

        [Column("votetotal")]
        public int VotesScore { get; set; }

        // ToDo: attach, similar, taglist

        [Column("lastposterid")]
        public int LastPostAuthorId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBUser LastPostAuthor { get; set; }

        /// <summary>
        /// Anzahl der Nutzer, die im Thema einen Beitrag geschrieben haben
        /// </summary>
        public int PosterCount { get; set; }

        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public List<VBPost> Replys { get; set; }

        public VBThread() { }

        public VBThread(int lastPostAuthorId, string lastPostAuthorName, int firstPostId, int lastPostId, int forumId, int authorId, string authorName, DateTime createdTime, DateTime lastPostTime, 
            string title, bool isOpen = true, bool isVisible = true, int views = 0, string notes = "", string prefixId="") {
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
        }
    }
}

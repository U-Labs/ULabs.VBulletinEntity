using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;
using ULabs.VBulletinEntity.Models.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Models.Forum {
    [Table("post")]
    [JsonObject(IsReference = true)]
    public class VBPost {
        [Column("postid"), Key]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("threadid")]
        public int ThreadId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBThread Thread { get; set; }

        [Column("parentid")]
        public int ParentPostId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBPost ParentPost { get; set; }

        [Column("username")]
        public string AuthorName { get; set; }

        [Column("userid")]
        public int? AuthorId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBUser Author { get; set; }

        [Column("dateline")]
        public int CreatedTimeRaw { get; set; }

        [NotMapped]
        public DateTime CreatedTime {
            get { return CreatedTimeRaw.ToDateTime(); }
            set { CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
        }

        [Column("pagetext")]
        public string Text { get; set; }

        [NotMapped]
        public string HtmlText { get; set; }

        [Column("allowsmilie")]
        public bool AllowSmilies { get; set; }

        [Column("showsignature")]
        public bool ShowSignature { get; set; }

        [Column("ipaddress")]
        public string IpAddress { get; set; } = "";

        [Column("iconid")]
        public int IconId { get; set; } = 0;

        [Column("visible")]
        public int VisibilityRaw { get; set; }

        [NotMapped]
        public VBPostVisibleState Visibility {
            get => (VBPostVisibleState)VisibilityRaw;
            set => VisibilityRaw = (int)value;
        }

        [Column("attach")]
        public bool HasAttachment { get; set; }

        [Column("infraction")]
        public int InfractionType { get; set; }

        [Column("reportthreadid")]
        public int ReportThreadId { get; set; }

        [Column("post_thanks_amount")]
        public int ThanksCount { get; set; }

        [Column("htmlstate")]
        public string PosterCount { get; set; } = "on_nl2br";

        public List<VBAttachment> Attachments { get; set; }

        public static string RemoveIncompatibleCharsFromText(string source) {
            string cleanText = Regex.Replace(source, @"\p{Cs}", "");
            return cleanText;
        }

        public VBPost() { }

        // ToDo: IP shouldn't be optional
        public VBPost(VBUser author, string title, string text, string ipAddress = "", int threadId = 0, bool allowSmilies = true, VBPostVisibleState visibility = VBPostVisibleState.Visible) {
            ThreadId = threadId;
            AuthorName = author.UserName;
            AuthorId = author.Id;
            Title = title;
            Text = text;
            IpAddress = IpAddress;
            CreatedTime = DateTime.Now;
            AllowSmilies = allowSmilies;

            Visibility = visibility;
        }
    }

    public enum VBPostVisibleState {
        Visible = 1,
        Deleted = 2
    }
}

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
        [Column("postid")]
        public int Id { get; set; }

        public int ThreadId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBThread Thread { get; set; }

        [Column("parentid")]
        public int ParentPostId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBPost ParentPost { get; set; }

        [Column("username"), MaxLength(100)]
        public string AuthorName { get; set; }

        [Column("userid")]
        public int? AuthorId { get; set; }
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public VBUser Author { get; set; }

        [MaxLength(250)]
        public string Title { get; set; }

        [Column("dateline")]
        public int CreatedTimeRaw { get; set; }

        [Column("pagetext")]
        public string Text { get; set; }

        [Column("allowsmilie")]
        public bool AllowSmilies { get; set; }

        [Column("showsignature")]
        public bool ShowSignature { get; set; }

        [Column("ipaddress")]
        public string IpAddress { get; set; }

        [Column("iconid")]
        public int IconId { get; set; } = 0;

        [Column("visible")]
        public int VisibilityRaw { get; set; }

        [Column("attach")]
        public bool HasAttachment { get; set; }

        [Column("infraction")]
        public int InfractionType { get; set; }

        [Column("reportthreadid")]
        public int ReportThreadId { get; set; }

        [Column("post_thanks_amount")]
        public int ThanksCount { get; set; }

        [Column("htmlstate")]
        public string HtmlStateRaw { get; set; } = "on_nl2br";

        public List<VBAttachment> Attachments { get; set; }

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

        [NotMapped]
        public VBPostHtmlState HtmlState {
            // Mapping OnNl2Br enum value to VBs raw on_nl2br
            get {
                string rawState = HtmlStateRaw;
                if(rawState.Contains("_")) {
                    rawState = "OnNl2Br"; 
                }
                return (VBPostHtmlState)Enum.Parse(typeof(VBPostHtmlState), rawState);
            }
            set {
                if(value == VBPostHtmlState.OnNl2Br) {
                    HtmlStateRaw = "on_nl2br";
                }else {
                    HtmlStateRaw= value.ToString().ToLower();
                }
            }
        }

        [NotMapped]
        public string HtmlText { get; set; }

        [NotMapped]
        public DateTime CreatedTime {
            get => CreatedTimeRaw.ToDateTime();
            set => CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        [NotMapped]
        public VBPostVisibleState Visibility {
            get => (VBPostVisibleState)VisibilityRaw;
            set => VisibilityRaw = (int)value;
        }

        public static string RemoveIncompatibleCharsFromText(string source) {
            string cleanText = Regex.Replace(source, @"\p{Cs}", "");
            return cleanText;
        }
    }

    public enum VBPostVisibleState {
        Visible = 1,
        Deleted = 2
    }

    public enum VBPostHtmlState {
        Off,
        On,
        OnNl2Br
    }
}

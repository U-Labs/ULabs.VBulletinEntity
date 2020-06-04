using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ULabs.VBulletinEntity.LightModels.User;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightModels.Forum {
    public class VBLightPost {
        public int Id { get; set; }
        public int ParentPostId { get; set; }
        public int ThreadId { get; set; }
        public VBLightUser Author { get; set; }
        public int CreatedTimeRaw { get; set; }
        public string Text { get; set; }
        public string IpAddress { get; set; }
        public int VisibilityRaw { get; set; }
        public bool HasAttachments { get; set; }
        public int ThanksCount { get; set; }

        public VBPostVisibleState Visibility {
            get => (VBPostVisibleState)VisibilityRaw;
            set => VisibilityRaw = (int)value;
        }

        public DateTime CreatedTime {
            get => CreatedTimeRaw.ToDateTime();
            set => CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        public string GetTextExcerpt(int wordsLimit = 15, string suffix = " ...") {
            string cleanText = Regex.Replace(Text, @"\[[^\]]+\]", "");
            string excerpt = string.Join(" ", cleanText.Split().Take(wordsLimit)) + suffix;
            return excerpt;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ULabs.LightVBulletinEntity.LightModels.User;

namespace ULabs.LightVBulletinEntity.LightModels.Forum {
    public class LightCreateThreadModel {
        public VBLightUser Author { get; set; }
        public int ForumId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string IpAddress { get; set; }
        public bool IsOpen { get; set; }

        public LightCreateThreadModel(VBLightUser author, int forumId, string title, string text, string ipAddress, bool isOpen = true) {
            Author = author;
            ForumId = forumId;
            Title = title;
            Text = text;
            IpAddress = ipAddress;
            IsOpen = isOpen;
        }
    }
}

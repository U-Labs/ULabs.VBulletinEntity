using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Models.User;

namespace ULabs.VBulletinEntity.Models.Manager {
    public class CreateReplyModel {
        public VBUser Author { get; set; }
        public int ThreadId { get; set; }
        public string Text { get; set; }
        public string IpAddress { get; set; }
        public string Title { get; set; }
        public bool UpdatePostCounter { get; set; }
        public CreateReplyModel(VBUser author, int threadId, string text, string ipAddress, string title = "", bool updatePostCounter = true) {
            Author = author;
            ThreadId = threadId;
            Text = text;
            IpAddress = ipAddress;
            Title = title;
            UpdatePostCounter = updatePostCounter;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using ULabs.LightVBulletinEntity.Tools;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.LightVBulletinEntity.LightModels.User {
    public class VBLightPrivateMessage {
        public int PmId { get; set; }
        public int PmTextId { get; set; }
        public int ParentPmId { get; set; }
        public int MessageReadRaw { get; set; }
        public VBLightUser FromUser { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int SendTimeRaw { get; set; }

        public VBPrivateMessageReadState MessageRead {
            get => (VBPrivateMessageReadState)Enum.Parse(typeof(VBPrivateMessageReadState), MessageReadRaw.ToString());
            set => MessageReadRaw = (int)value;
        }
        public DateTime SendTime {
            get => DateTimeExtensions.ToDateTime(SendTimeRaw);
            set => SendTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }
    }

    public enum VBPrivateMessageReadState {
        Unread = 0,
        Read = 1,
        Answered = 2
    }
}

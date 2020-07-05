using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.VBulletinEntity.LightModels.Forum {
    public class VBLightAutosave {
        static Dictionary<string, VBLightAutosaveContentType> contentTypes = new Dictionary<string, VBLightAutosaveContentType>() {
            { "vBBlog_BlogComment", VBLightAutosaveContentType.BlogComment },
            { "vBBlog_BlogEntry", VBLightAutosaveContentType.Infraction },
            { "vBForum_Infraction", VBLightAutosaveContentType.Infraction },
            { "vBForum_Post", VBLightAutosaveContentType.Post },
            { "vBForum_PrivateMessage", VBLightAutosaveContentType.PrivateMessage },
            { "vBForum_Signature", VBLightAutosaveContentType.Signature },
            { "vBForum_Thread", VBLightAutosaveContentType.Thread },
            { "vBForum_UserNote", VBLightAutosaveContentType.UserNote },
            { "vBForum_VisitorMessage", VBLightAutosaveContentType.VisitorMessage }
        };
        VBLightAutosaveContentType contentType;
        byte[] contentTypeRaw;

        /// <summary>
        /// Converting enum to VARBINARY from VBs DB column
        /// </summary>
        /// <param name="contentType">Enum content type to convert</param>
        /// <returns></returns>
        public static byte[] ContentTypeToByteArray(VBLightAutosaveContentType contentType) {
            string plainKey = contentTypes.FirstOrDefault(c => c.Value == contentType).Key;
            return Encoding.UTF8.GetBytes(plainKey);
        }
        public VBLightAutosaveContentType ContentType {
            get => contentType;
            set {
                contentType = value;
                contentTypeRaw = ContentTypeToByteArray(contentType);
            }
        }
        public byte[] ContentTypeRaw { 
            get => contentTypeRaw;
            set {
                contentTypeRaw = value;

                string plainKey = Encoding.UTF8.GetString(contentTypeRaw);
                contentType = contentTypes[plainKey];
            }
        }
        public int ParentContentId { get; set; }
        public int ContentId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; }
        public int CreatedTimeRaw { get; set; }
        public DateTime CreatedTime {
            get => CreatedTimeRaw.ToDateTime();
            set => CreatedTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value);
        }

        /// <summary>
        /// Constructor for saving a post
        /// </summary>
        /// <param name="threadId">Id of the thread</param>
        /// <param name="userId">Id of the user author</param>
        /// <param name="text">Text to save</param>
        public VBLightAutosave(int threadId, int userId, string text) {
            ContentType = VBLightAutosaveContentType.Post;
            ParentContentId = threadId;
            UserId = userId;
            Text = text;
            CreatedTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor for Dapper and/or manual property setting
        /// </summary>
        public VBLightAutosave() { }
    }

    public enum VBLightAutosaveContentType {
        BlogComment,
        BlogEntry,
        Infraction,
        Post,
        Thread,
        PrivateMessage,
        Signature,
        UserNote,
        VisitorMessage
    }
}

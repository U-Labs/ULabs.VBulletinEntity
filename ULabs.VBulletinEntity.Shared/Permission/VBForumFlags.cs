using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.Shared.Permission {
    /// <summary>
    /// Forum permission from includes/xml/bitfield_vbulletin.xml
    /// </summary>
    // ToDo: Implement other bitfields from the xml file
    [Flags]
    public enum VBForumFlags : int {
        None = 0,
        CanViewForum = 1,
        CanViewThreads = 524288,
        CanViewOtherThreads = 2,
        CanSearchForums = 4,
        CanEmailToFriends = 8,
        CanCreateThreads = 16,
        CanReplyToOwnThreads = 32,
        CanReplyToOtherThreads = 64,
        CanEditOwnPosts = 128,
        CanDeleteOwnPosts = 256,
        CanDeleteOwnThreads = 512,
        CanOpenCloseOwnThreads = 1024, 
        CanMoveOwnThreads = 2048,
        CanDownloadAttachments = 4096,
        CanUploadAttachments = 8192,
        CanPostPolls = 16384,
        CanVoteOnPolls = 32768,
        CanRateThreads = 65536,
        FollowForumModerationRules = 131072,
        CanSeeDeletionNotices = 262144,
        CanTagOwnThreads = 1048576,
        CanTagOtherThreads = 2097152,
        CanDeleteTagsOnOwnThreads = 4194304,
        CanSeeThumbnails = 8388608,
        CanCssAttachments = 16777216,
        BypassDoublePosts = 33554432,
        CanWriteMembers = 67108864
    }
}

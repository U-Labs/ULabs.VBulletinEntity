using System;

namespace ULabs.VBulletinEntity.Models.Permission {
    /// <summary>
    /// Mod permission from includes/xml/bitfield_vbulletin.xml
    /// Only for non global mods (user.php?do=edit&u=182), seems that they're stored in moderator table with moderatorpermissions2
    /// ToDo: Currently not used. We leave it here for later implementation if required
    /// </summary>
    [Flags]
    public enum VBModeratorFlags : int {
        None = 0,
        CanEditPosts = 1 << 0, // 1
        CanDeletePosts = 1 << 1, // 2
        CanOpenClose = 1 << 2, // 4
        CanEditThreads = 1 << 3, // 8
        CanManageThreads = 1 << 4, // ...
        CanAnnounce = 1 << 5,
        CanModeratePosts = 1 << 6,
        CanModerateAttachments = 1 << 7,
        CanMassMove = 1 << 8,
        CanMassPrune = 1 << 9,
        CanViewIps = 1 << 10,
        CanViewProfile = 1 << 11,
        CanBanUsers = 1 << 12,
        CanUnbanUsers = 1 << 13,
        NewThreadEmail = 1 << 14,
        NewPostEmail = 1 << 15,
        CanSetPassword = 1 << 16,
        CanRemovePosts = 1 << 17,
        CanEditSignatures = 1 << 18,
        CanEditAvatar = 1 << 19,
        CanEditPoll = 1 << 20,
        CanEditProfilePic = 1 << 21,
        CanEditReputation = 1 << 22
    }
}

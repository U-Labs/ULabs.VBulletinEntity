using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ULabs.VBulletinEntity.LightManager;
using ULabs.VBulletinEntity.LightModels;
using ULabs.VBulletinEntity.LightModels.Forum;
using ULabs.VBulletinEntity.Models.Permission;

namespace ULabs.VBulletinEntityDemo.Models {
    public class LightDashboardModel {
        public List<VBLightThread> NewestThreads { get; set; }
        public List<VBLightUnreadActiveThread> UnreadActiveThreads { get; set; }
        public List<VBLightForum> NotViewableCategories { get; set; }
        public List<VBLightForum> ViewableCategories { get; set; }
        public Dictionary<VBLightForum, VBForumFlags> CategoryPermissions { get; set; }
        public List<VBLightPostThanks> RecentThanks { get; set; }
        public LightDashboardModel(VBLightForumManager lightForumManager, VBLightThreadManager lightThreadManager, VBLightSessionManager lightSessionManager) {
            var session = lightSessionManager.GetCurrent();
            var forumsCanRead = lightForumManager.GetForumsWhereUserCan(session.User.PrimaryUserGroupId, VBForumFlags.CanViewThreads);

            NewestThreads = lightThreadManager.GetNewestThreads(includedForumIds: forumsCanRead.Select(f => f.Id).ToList());
            NotViewableCategories = lightForumManager.GetForumsWhereUserCanNot(session.User.PrimaryUserGroupId, VBForumFlags.CanViewForum, onlyParentCategories: true);
            ViewableCategories = lightForumManager.GetForumsWhereUserCan(session.User.PrimaryUserGroupId, VBForumFlags.CanViewForum, onlyParentCategories: true);
            CategoryPermissions = lightForumManager.GetPermissions(userGroupId: 2, onlyParentCategories: true);

            if (session.IsLoggedIn) {
                UnreadActiveThreads = lightThreadManager.GetUnreadActiveThreads(session.UserId);
                RecentThanks = lightThreadManager.GetThanks(session.UserId, afterTimestamp: session.User.LastActivityRaw);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ULabs.LightVBulletinEntity.LightManager;
using ULabs.LightVBulletinEntity.LightModels;
using ULabs.LightVBulletinEntity.LightModels.Forum;
using ULabs.VBulletinEntity.Shared.Permission;

namespace ULabs.VBulletinEntityDemo.Models {
    public class LightDashboardModel {
        public List<VBLightThread> NewestThreads { get; set; }
        public List<VBLightForum> NotViewableCategories { get; set; }
        public List<VBLightForum> ViewableCategories { get; set; }
        public Dictionary<VBLightForum, VBForumFlags> CategoryPermissions { get; set; }
        // Could both be null on guest users
        public List<VBLightUnreadActiveThread> UnreadActiveThreads { get; set; } = new List<VBLightUnreadActiveThread>();
        public List<VBLightPostThanks> RecentThanks { get; set; } = new List<VBLightPostThanks>();
        public LightDashboardModel(VBLightForumManager lightForumManager, VBLightThreadManager lightThreadManager, VBLightSessionManager lightSessionManager) {
            var session = lightSessionManager.GetCurrent();
            var forumsCanRead = lightForumManager.GetForumsWhereUserCan(session.User.PrimaryUserGroup.Id, VBForumFlags.CanViewThreads);

            NewestThreads = lightThreadManager.GetNewestThreads(includedForumIds: forumsCanRead.Select(f => f.Id).ToList());
            NotViewableCategories = lightForumManager.GetForumsWhereUserCanNot(session.User.PrimaryUserGroup.Id, VBForumFlags.CanViewForum, onlyParentCategories: true);
            ViewableCategories = lightForumManager.GetForumsWhereUserCan(session.User.PrimaryUserGroup.Id, VBForumFlags.CanViewForum, onlyParentCategories: true);
            CategoryPermissions = lightForumManager.GetPermissions(userGroupId: 2, onlyParentCategories: true);

            if (session.IsLoggedIn) {
                UnreadActiveThreads = lightThreadManager.GetUnreadActiveThreads(session.User.Id, ignoredThreadIds: new List<int>() { 29780 });
                var allUnreadActiveThreads= lightThreadManager.GetUnreadActiveThreads(session.User.Id, count:99);
                int countUnreadActiveThreads = lightThreadManager.CountUnreadActiveThreads(session.User.Id, ignoredThreadIds: new List<int>() { 29780 });
                int countAllUnreadActiveThreads = lightThreadManager.CountUnreadActiveThreads(session.User.Id);

                RecentThanks = lightThreadManager.GetThanks(session.User.Id);
            }
        }
    }
}

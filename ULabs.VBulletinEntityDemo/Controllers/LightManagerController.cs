using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity.Attributes;
using ULabs.VBulletinEntity.LightManager;
using ULabs.VBulletinEntity.Manager;
using ULabs.VBulletinEntity.Models.Permission;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class LightManagerController : Controller {
        readonly VBLightDashboardManager lightDashboardManager;
        readonly VBLightSessionManager lightSessionManager;
        readonly VBLightForumManager lightForumManager;
        readonly VBLightThreadManager lightThreadManager;
        public LightManagerController(VBLightDashboardManager lightDashboardManager, VBLightSessionManager lightSessionManager, VBLightForumManager lightForumManager, VBLightThreadManager lightThreadManager) {
            this.lightDashboardManager = lightDashboardManager;
            this.lightSessionManager = lightSessionManager;
            this.lightForumManager = lightForumManager;
            this.lightThreadManager = lightThreadManager;
        }
        public IActionResult Dashboard() {
            var session = lightSessionManager.GetCurrent();
            //var nonViewable = lightDashboardManager.GetForumIdsWithoutViewPermission(2);
            var newestThreads = lightDashboardManager.GetNewestThreads(10, excludedForumIds: new List<int>() { 16 }, orderByLastPostDate: false);
            //var newestThreads2 = lightDashboardManager.GetNewestThreads(10, orderByLastPostDate: true);
            var newReplys = lightDashboardManager.GetUnreadActiveThreads(18, count: 99, ignoredForumIds: new List<int> { 16 });
            lightDashboardManager.MarkContentAsRead(contentId: 39886, userId: 18);

            var perm = lightForumManager.GetPermission(userGroupId: 5, forumId: 151);
            var forumsCanRead = lightForumManager.GetForumsWhereUserCan(userGroupId: 2, VBForumFlags.CanViewForum);
            var forumsCanWrite = lightForumManager.GetForumsWhereUserCan(userGroupId: 9, VBForumFlags.CanCreateThreads);
            var diffForums = forumsCanWrite.Where(x => !forumsCanRead.Any(y => y.ForumId == x.ForumId))
                .ToList();

            var noViewable2 = lightForumManager.GetForumsWhereUserCanNot(userGroupId: 2, VBForumFlags.CanViewForum, onlyParentCategories: true);
            var allPerm = lightForumManager.GetPermissions(userGroupId: 2);
            var parentPerm = lightForumManager.GetPermissions(userGroupId: 2, onlyParentCategories: true);

            var forum = lightForumManager.Get(forumId: 151);

            var thanks = lightThreadManager.GetThanks(userId: 18);
            var recentThanks = lightThreadManager.GetThanks(userId: 18, afterTimestamp: 1566338801);
            return View(newestThreads);
        }
        [VBLightAuthorize]
        public IActionResult Authorized() {
            return Content("Youre authorized!");
        }
    }
}
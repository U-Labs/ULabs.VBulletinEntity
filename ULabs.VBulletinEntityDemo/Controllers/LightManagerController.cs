using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity.Attributes;
using ULabs.VBulletinEntity.LightManager;
using ULabs.VBulletinEntity.Manager;
using ULabs.VBulletinEntity.Models.Permission;
using ULabs.VBulletinEntityDemo.Models;

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
        public IActionResult NewestThreads() {
            var session = lightSessionManager.GetCurrent();
            var forumsCanRead = lightForumManager.GetForumsWhereUserCan(userGroupId: session.User.PrimaryUserGroupId, VBForumFlags.CanViewForum);
            var newestThreads = lightDashboardManager.GetNewestThreads();
            return View(newestThreads);
        }
        public IActionResult Dashboard() {
            var session = lightSessionManager.GetCurrent();
            lightDashboardManager.MarkContentAsRead(contentId: 39886, userId: 18);

            var thanks = lightThreadManager.GetThanks(userId: 18);
            var recentThanks = lightThreadManager.GetThanks(userId: 18, afterTimestamp: 1566338801);
            

            var model = new LightDashboardModel(lightDashboardManager, lightForumManager, lightThreadManager, lightSessionManager);

            lightSessionManager.UpdateLastActivity(session.SessionHash, "/LightTest");
            return View(model);
        }
        [VBLightAuthorize]
        public IActionResult Authorized() {
            return Content("Youre authorized!");
        }
    }
}
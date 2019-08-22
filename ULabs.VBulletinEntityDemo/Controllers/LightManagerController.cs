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
        readonly VBLightSessionManager lightSessionManager;
        readonly VBLightForumManager lightForumManager;
        readonly VBLightThreadManager lightThreadManager;
        public LightManagerController(VBLightSessionManager lightSessionManager, VBLightForumManager lightForumManager, VBLightThreadManager lightThreadManager) {
            this.lightSessionManager = lightSessionManager;
            this.lightForumManager = lightForumManager;
            this.lightThreadManager = lightThreadManager;
        }
        public IActionResult Dashboard() {
            var session = lightSessionManager.GetCurrent();
            var model = new LightDashboardModel(lightForumManager, lightThreadManager, lightSessionManager);

            lightSessionManager.UpdateLastActivity(session.SessionHash, "/LightTest");
            return View(model);
        }
        public IActionResult ViewThread(int id) {
            var model = new ViewThreadModel(lightThreadManager, id, 1);
            return View(model);
        }
        [VBLightAuthorize]
        public IActionResult Authorized() {
            return Content("Youre authorized!");
        }
    }
}
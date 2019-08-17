using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity.LightManager;
using ULabs.VBulletinEntity.Manager;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class LightManagerController : Controller {
        readonly VBLightDashboardManager lightDashboardManager;
        readonly VBLightSessionManager lightSessionManager;
        public LightManagerController(VBLightDashboardManager lightDashboardManager, VBLightSessionManager lightSessionManager) {
            this.lightDashboardManager = lightDashboardManager;
            this.lightSessionManager = lightSessionManager;
        }
        public IActionResult Dashboard() {
            var session = lightSessionManager.GetCurrent();
            var newestThreads = lightDashboardManager.GetNewestThreads(10);
            return View(newestThreads);
        }
    }
}
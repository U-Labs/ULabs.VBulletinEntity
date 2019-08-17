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
        public LightManagerController(VBLightDashboardManager lightDashboardManager) {
            this.lightDashboardManager = lightDashboardManager;
        }
        public async Task<IActionResult> Dashboard() {
            var newestThreads = lightDashboardManager.GetNewestThreads(10);
            return View(newestThreads);
        }
    }
}
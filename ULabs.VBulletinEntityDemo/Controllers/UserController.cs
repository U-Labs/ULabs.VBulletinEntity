using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity.LightManager;
using ULabs.VBulletinEntity.Manager;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class UserController : Controller {
        readonly VBUserManager userManager;
        readonly VBSessionManager sessionManager;
        readonly VBLightDashboardManager lightDashboardManager;
        public UserController(VBUserManager userManager, VBSessionManager sessionManager, VBLightDashboardManager lightDashboardManager) {
            this.userManager = userManager;
            this.sessionManager = sessionManager;
            this.lightDashboardManager = lightDashboardManager;
        }
        public async Task<IActionResult> Profile(int id) {
            var user = await userManager.GetUserAsync(id);
            return View(user);
        }
        public async Task<IActionResult> Session() {
            var session = await sessionManager.GetCurrentAsync();
            var notViewableForumIds = lightDashboardManager.GetForumIdsWithoutViewPermission(session.User.UserGroupId);
            var categoriesWithChilds = lightDashboardManager.GetCategoriesWithChilds();
            return View(session);
        }
    }
}
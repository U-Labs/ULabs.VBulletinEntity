using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity.Manager;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class UserController : Controller {
        readonly VBUserManager userManager;
        readonly VBSessionManager sessionManager;
        public UserController(VBUserManager userManager, VBSessionManager sessionManager) {
            this.userManager = userManager;
            this.sessionManager = sessionManager;
        }
        public async Task<IActionResult> Profile(int id) {
            var user = await userManager.GetUserAsync(id);
            return View(user);
        }
        public async Task<IActionResult> Session() {
            var session = await sessionManager.GetCurrentAsync();
            return View(session);
        }
    }
}
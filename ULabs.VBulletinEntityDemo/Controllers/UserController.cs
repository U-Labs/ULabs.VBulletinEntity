using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity.Manager;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class UserController : Controller {
        readonly VBUserManager userManager;
        public UserController(VBUserManager userManager) {
            this.userManager = userManager;
        }
        public async Task<IActionResult> Profile(int id) {
            var user = await userManager.GetUser(id);
            return View(user);
        }
    }
}
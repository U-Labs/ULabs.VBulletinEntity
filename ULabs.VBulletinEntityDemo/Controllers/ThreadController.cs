using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity.Manager;
using ULabs.VBulletinEntity.Models.Forum;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class ThreadController : Controller {
        readonly VBThreadManager threadManager;
        readonly VBUserManager userManager;
        public ThreadController(VBThreadManager threadManager, VBUserManager userManager) {
            this.threadManager = threadManager;
            this.userManager = userManager;
        }
        public async Task<IActionResult> View(int id) {
            var thread = await threadManager.GetThreadAsync(id);
            var replys = await threadManager.GetReplysAsync(id);
            var model = new KeyValuePair<VBThread, List<VBPost>>(thread, replys);
            return View(model);
        }

        public async Task<IActionResult> Create(int userId, int forumId, string title, string text) {
            var user = await userManager.GetUserAsync(userId);
            var clientIp = HttpContext.Connection.RemoteIpAddress;
            var thread = await threadManager.CreateThreadAsync(user, clientIp.ToString(), forumId, title, text);
            return Json(thread);
        }
    }
}
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
        public ThreadController(VBThreadManager threadManager) {
            this.threadManager = threadManager;
        }
        public async Task<IActionResult> View(int id) {
            var thread = await threadManager.GetThreadAsync(id);
            var replys = await threadManager.GetReplysAsync(id);
            var model = new KeyValuePair<VBThread, List<VBPost>>(thread, replys);
            return View(model);
        }
    }
}
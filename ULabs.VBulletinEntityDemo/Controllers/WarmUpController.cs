using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ULabs.VBulletinEntity;
using ULabs.VBulletinEntity.Manager;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class WarmUpController : Controller {
        readonly VBDbContext db;
        readonly VBThreadManager threadManager;
        readonly VBSessionManager sessionManager;
        readonly VBSettingsManager settingsManager;
        readonly VBForumManager forumManager;
        readonly VBUserManager userManager;

        public WarmUpController(VBDbContext db, VBThreadManager threadManager, VBSessionManager sessionManager, VBSettingsManager settingsManager, VBForumManager forumManager, VBUserManager userManager) {
            this.db = db;
            this.threadManager = threadManager;
            this.sessionManager = sessionManager;
            this.settingsManager = settingsManager;
            this.forumManager = forumManager;
            this.userManager = userManager;
        }

        public ActionResult Index() {
            DatabaseWarmUp.WarmUpServices(db, threadManager, sessionManager, settingsManager, forumManager, userManager);
            return View();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity;
using ULabs.VBulletinEntityDemo.Models;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class HomeController : Controller {
        readonly VBDbContext db;
        public HomeController(VBDbContext db) {
            this.db = db;
        }
        public IActionResult Index() {
            return NewestContent();
        }
        public IActionResult NewestContent(int limit = 10) {
            var model = new NewestContentModel(db, limit);
            return View(model);
        }
    }
}

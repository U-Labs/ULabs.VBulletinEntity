using System;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity;
using ULabs.VBulletinEntityDemo.Models;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class DbContextController : Controller {
        readonly VBDbContext db;
        public DbContextController(VBDbContext db) {
            this.db = db;
        }
        public IActionResult NewestContent(int limit = 10) {
            var model = new NewestContentModel(db, limit);
            return View(model);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class DbContextController : Controller {
        readonly VBDbContext db;
        public DbContextController(VBDbContext db) {
            this.db = db;
        }
        public IActionResult NewestThreads(int limit = 20) {
            var threads = db.Threads.OrderByDescending(thread => thread.Id)
                .Take(limit)
                .ToList();
            return View(threads);
        }
    }
}
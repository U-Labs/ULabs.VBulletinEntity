using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ULabs.VBulletinEntity;
using ULabs.VBulletinEntityDemo.Models;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class DbContextController : Controller {
        readonly VBDbContext db;
        public DbContextController(VBDbContext db) {
            this.db = db;
        }
        public IActionResult NewestContent(int limit = 20) {
            var model = new NewestContentModel();
            model.Threads = db.Threads.OrderByDescending(thread => thread.CreatedTimeRaw)
                .Include(thread => thread.Forum)
                .Take(limit)
                .ToList();
            model.Posts = db.Posts.OrderByDescending(post => post.CreatedTimeRaw)
                .Take(limit)
                .ToList();
            model.Users = db.Users.OrderByDescending(user => user.JoinDateRaw)
                .Take(limit)
                .ToList();
            return View(model);
        }
    }
}
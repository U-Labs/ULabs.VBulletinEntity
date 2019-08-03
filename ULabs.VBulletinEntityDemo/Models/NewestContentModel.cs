using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using ULabs.VBulletinEntity;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Models.User;

namespace ULabs.VBulletinEntityDemo.Models {
    public class NewestContentModel {
        public List<VBThread> Threads { get; set; }
        public List<VBPost> Posts { get; set; }
        public List<VBUser> Users { get; set; }

        public NewestContentModel(VBDbContext db, int limit) {
            Threads = db.Threads.OrderByDescending(thread => thread.CreatedTimeRaw)
                .Include(thread => thread.Forum)
                .Take(limit)
                .ToList();
            Posts = db.Posts.OrderByDescending(post => post.CreatedTimeRaw)
                .Take(limit)
                .ToList();
            Users = db.Users.OrderByDescending(user => user.JoinDateRaw)
                .Take(limit)
                .ToList();
            var groups = db.UserGroups.ToList();
            var testUser = db.Users.FirstOrDefault(u => u.Id == 7);
            bool has = testUser.Birthday.HasValue;
            var someUsers = db.Users.Take(30).ToList();
        }
    }
}
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
        public List<VBSession> Sessions { get; set; }

        public NewestContentModel(VBDbContext db, int limit) {
            Threads = db.Threads.OrderByDescending(thread => thread.CreatedTimeRaw)
                .Include(thread => thread.Forum)
                .Take(limit)
                .ToList();
            Posts = db.Posts.OrderByDescending(post => post.CreatedTimeRaw)
                .Include(post => post.Thread)
                .Take(limit)
                .ToList();
            Users = db.Users.OrderByDescending(user => user.JoinDateRaw)
                .Include(user => user.UserGroup)
                .Take(limit)
                .ToList();
            Sessions = db.Sessions.OrderByDescending(session => session.LastActivityRaw)
                .Include(session => session.User)
                .Include(session => session.InThread)
                .Take(limit)
                .ToList();
            var pms = db.Messages.OrderByDescending(m => m.Id)
                .Include(m => m.Text)
                .Include(m => m.ParentMessage)
                .Take(20)
                .ToList();
            var settings = db.Settings.ToList();
            var forums = db.Forums.Include(f => f.Parent)
                .Include(f => f.LastPostAuthor)
                .Include(f => f.Permissions)
                .ToList();
        }
    }
}
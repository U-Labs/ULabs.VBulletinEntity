using Microsoft.EntityFrameworkCore;
using System;
using ULabs.VBulletinEntity.Models.Forum;

namespace ULabs.VBulletinEntity {
    public class VBDbContext : DbContext {
        public VBDbContext(DbContextOptions options) : base(options) { }
        #region Forum
        public DbSet<VBThread> Threads { get; set; }
        public DbSet<VBPost> Posts { get; set; }
        #endregion
    }
}

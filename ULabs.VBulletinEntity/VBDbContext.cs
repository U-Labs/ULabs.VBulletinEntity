using Microsoft.EntityFrameworkCore;
using System;
using ULabs.VBulletinEntity.Models.AddOns;
using ULabs.VBulletinEntity.Models.Config;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Models.Message;
using ULabs.VBulletinEntity.Models.User;

namespace ULabs.VBulletinEntity {
    public class VBDbContext : DbContext {
        public VBDbContext(DbContextOptions options) : base(options) { }

        public DbSet<VBUser> Users { get; set; }
        public DbSet<VBUserGroup> UserGroups { get; set; }
        public DbSet<VBSession> Sessions { get; set; }
        public DbSet<VBCustomAvatar> CustomAvatars { get; set; }

        public DbSet<VBPost> Posts { get; set; }
        public DbSet<VBThread> Threads { get; set; }
        public DbSet<VBForum> Forums { get; set; }
        public DbSet<VBForumPermission> ForumPermissions { get; set; }

        public DbSet<VBPoll> Polls { get; set; }
        public DbSet<VBMessage> Messages { get; set; }
        public DbSet<VBMessageText> MessagesText { get; set; }
        public DbSet<VBAttachment> Attachments { get; set; }
        public DbSet<VBThreadRead> ThreadReads { get; set; }

        public DbSet<VBSettings> Settings { get; set; }

        public DbSet<PostThanks> PostThanks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            // Since vB has multiple FKs to VBPost (e.g. first/last post) we need to tell EF which of them is used for our 1:n mapping (thread/replys)
            builder.Entity<VBPost>()
                .HasOne(p => p.Thread)
                .WithMany(t => t.Replys)
                .HasForeignKey(x => x.ThreadId);

            builder.Entity<VBPost>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId);

            // UserId zero got set when deleting user
            builder.Entity<VBPost>()
                .Property(p => p.AuthorId)
                .HasDefaultValue(0);

            // Specify polls as optional so that thread doesn't got null when it has no poll
            builder.Entity<VBThread>()
                .HasOne(t => t.Poll)
                .WithMany()
                .HasForeignKey(t => t.PollId);

            // Solves the problem that a poll can be optional but vB doesn't allow null values here (only decimal 0) which cause conflicts to EF behavior. 
            // The trick is: Make PollId nullable and set default value to decimal 0. So EF use 0 instead of null.
            builder.Entity<VBThread>()
                .Property(t => t.PollId)
                .HasDefaultValue(0);

            builder.Entity<VBThread>()
                .HasOne(t => t.FirstPost)
                .WithMany()
                .HasForeignKey(t => t.FirstPostId);

            builder.Entity<VBThreadRead>()
                .HasKey(r => new { r.ThreadId, r.UserId });

            builder.Entity<VBThread>()
                .HasOne(t => t.LastPost)
                .WithMany()
                .HasForeignKey(t => t.LastPostId);

            builder.Entity<VBPost>()
                .HasMany(p => p.Attachments)
                .WithOne()
                .HasForeignKey(attach => attach.ContentId);

            builder.Entity<VBForum>()
                .HasMany(f => f.Permissions)
                //.WithOne(perm => perm.Forum)
                .WithOne()
                .HasForeignKey(p => p.ForumId);
            builder.Entity<VBForum>()
                .HasOne(f => f.Parent)
                .WithOne()
                .HasForeignKey<VBForum>(f => f.ParentId);
            builder.Entity<VBForum>()
                .Property(f => f.ParentId)
                .HasDefaultValue(-1);

            // Define custom avatar optional. Per default it would be required so that no posts were loaded when their authors haven't a custom avatar
            builder.Entity<VBUser>()
                .HasOne(u => u.CustomAvatar)
                .WithOne()
                .HasForeignKey<VBCustomAvatar>(a => a.UserId);

            SetNamingConventions(builder);
            // ToDo: Most examples in this guide show configurations being applied in the OnModelCreating method, but it is recommended to separate configurations out to individual files per entity
            // https://www.learnentityframeworkcore.com/configuration/fluent-api
            base.OnModelCreating(builder);
        }

        /// <summary>
        /// VB naming conventions: All lowercase (tables/columns) without any delimiter(parentpmid instead of ParentPmId as an example)
        /// </summary>
        void SetNamingConventions(ModelBuilder builder) {
            foreach (var entityType in builder.Model.GetEntityTypes()) {
                var relationalEntity = entityType.Relational();
                entityType.Relational().TableName = entityType.Relational().TableName.ToLower();

                foreach (var property in entityType.GetProperties()) {
                    property.Relational().ColumnName = property.Relational().ColumnName.ToLower();
                }
            }
        }
    }
}

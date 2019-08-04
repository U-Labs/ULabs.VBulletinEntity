using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ULabs.VBulletinEntity.Models.Permission;
using ULabs.VBulletinEntity.Models.User;

namespace ULabs.VBulletinEntity.Manager {
    public class VBUserManager {
        readonly VBDbContext db;

        public VBUserManager(VBDbContext db) {
            this.db = db;
        }

        public async Task<VBUser> GetUser(int id) {
            var user = await GetUser<int>(u => u.Id == id);
            return user?.FirstOrDefault();
        }

        public async Task<List<VBUser>> GetUser<TOrderKey>(Expression<Func<VBUser, bool>> predicate, Expression<Func<VBUser, TOrderKey>> orderBy = null, bool descOrder = false, int limit = 50) {
            var query = db.Users.Include(u => u.UserGroup)
                .Include(u => u.DisplayGroup)
                .Where(predicate);

            IOrderedQueryable<VBUser> orderedQuery = query.OrderBy(u => u.Id);
            if (orderBy != null) {
                if (descOrder) {
                    orderedQuery = query.OrderByDescending(orderBy);
                } else {
                    orderedQuery = query.OrderBy(orderBy);
                }
            }

            var users = await orderedQuery.Take(limit)
                .ToListAsync();
            return users;
        }

        public async Task<VBUserGroup> GetUserGroup(int id) {
            var group = await db.UserGroups.FindAsync(id);
            return group;
        }

        public async Task<List<VBUserGroup>> GetUserGroups(List<int> ids) {
            var groups = await db.UserGroups.Where(g => ids.Contains(g.Id))
                .ToListAsync();
            return groups;
        }

        public async Task IncrementPostCounterAsync(int userId, int lastPostId, DateTime lastPostDateTime, bool saveChanges = true) {
            var user = await db.Users.FindAsync(userId);
            if (user == null) {
                throw new Exception($"No user exists with id #{userId}!");
            }

            user.LastActivityTime = user.LastVisitTime = DateTime.UtcNow;
            user.LastPostTime = lastPostDateTime;
            user.LastPostId = lastPostId;
            user.PostsCount++;

            if (saveChanges) {
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> IsGlobalModeratorAsync(VBUser user) {
            if(user.UserGroup.AdminPermissions.HasFlag(VBAdminFlags.IsModerator)) {
                return true;
            }

            var allGroups = await GetAllMemberGroups(user);
            return allGroups.Any(group => group.AdminPermissions.HasFlag(VBAdminFlags.IsModerator));
        }

        public async Task<List<VBUserGroup>> GetAllMemberGroups(VBUser user) {
            if (user.UserGroup == null) {
                throw new Exception($"UserGroup is null for {user.Id}");
            }

            var groups = new List<VBUserGroup>() { user.UserGroup };
            if(user.MemberGroupIds.Any()) {
                // ToDo: Load all groups in one query for better performance
                var memberGroups = await GetUserGroups(user.MemberGroupIds);
                groups.AddRange(memberGroups);
            }
            return groups;
        }

    }
}

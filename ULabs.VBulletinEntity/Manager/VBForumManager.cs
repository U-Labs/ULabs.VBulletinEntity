using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ULabs.VBulletinEntity.Caching;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Models.Permission;
using ULabs.VBulletinEntity.Models.User;

namespace ULabs.VBulletinEntity.Manager {
    public class VBForumManager {
        readonly VBDbContext db;
        readonly IVBCache cache;

        public VBForumManager(VBDbContext db, IVBCache cache) {
            this.db = db;
            this.cache = cache;
        }

        IQueryable<VBForum> GetForumsQuery(bool writeable = false) {
            var query = db.Forums.Include(f => f.Parent)
                    // Required to fetch correct permission for nested child forums
                    .ThenInclude(f => f.Permissions)
                .Include(f => f.Permissions)
                .AsQueryable();

            if (!writeable) {
                query = query.AsNoTracking();
            }
            return query;
        }
        IQueryable<VBForumPermission> GetForumPermissionQuery(IEnumerable<int> userGroupIds, int forumId, bool writeable = false) {
            var query = db.ForumPermissions.Where(p => p.ForumId == forumId && userGroupIds.Contains(p.UserGroupId));
            if (!writeable) {
                query = query.AsNoTracking();
            }
            return query;
        }
        public async Task<VBForumPermission> GetForumPermissionAsync(int userGroupId, int forumId, bool writeable = false) {
            return await GetForumPermissionQuery(new List<int>() { userGroupId }, forumId, writeable).SingleOrDefaultAsync();
        }
        public async Task<List<VBForumPermission>> GetForumPermissionAsync(IEnumerable<int> userGroupIds, int forumId, bool writeable = false) {
            return await GetForumPermissionQuery(userGroupIds, forumId, writeable).ToListAsync();
        }

        public async Task<Dictionary<VBForum, List<int>>> GetCategoriesWithChildIdsAsync(VBUserGroup userGroup, VBForumFlags flags = VBForumFlags.CanViewForum | VBForumFlags.CanViewThreads) {
            var forums = await GetCategoriesWhereUserCanAsync(userGroup, flags);
            var forumsWithChilds = forums.Select(f => new {
                Forum = f,
                Childs = new List<int>(f.ChildList) { f.Id }
            }).GroupBy(x => x.Forum)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.SelectMany(x => x.Childs).ToList());
            return forumsWithChilds;
        }

        public async Task<List<VBForum>> GetCategoriesWhereUserCanAsync(VBUserGroup userGroup, VBForumFlags flags = VBForumFlags.CanViewForum | VBForumFlags.CanViewThreads) {
            var forums = await GetForumsWhereUserCanAsync(userGroup, flags);
            var categories = forums.Where(f => f.ParentId == -1)
                .ToList();
            return categories;
        }

        public async Task<bool> UserCanInForum(int forumId, VBUserGroup userGroup, VBForumFlags flags) {
            // Since we only check if the user has permission, writing and tracking is never required here
            var forum = await GetForumsQuery().FirstOrDefaultAsync(f => f.Id == forumId);
            var highestPermission = FetchHighestPermissionFlagForUser(forum, userGroup);
            return highestPermission.HasFlag(flags);
        }

        public async Task<List<VBForum>> GetForumsWhereUserCanAsync(VBUserGroup userGroup, VBForumFlags? flags = null, bool writeable = false) {
            // Better not caching permissions here. We ran into strange issues, altough we used the group id as sub key for caching
            if (writeable || !cache.TryGet(VBCacheKey.Forums, out List<VBForum> forums)) {
                forums = await GetForumsQuery().ToListAsync();
                cache.Set(VBCacheKey.Forums, forums);
            }

            var forumsWithFlags = FetchForumsWithFlags(forums, userGroup);
            var permittedForums = forumsWithFlags.Where(kvp => flags == null || kvp.Value.HasFlag(flags.Value))
                .Select(kvp => kvp.Key)
                .ToList();

            return permittedForums;
        }

        public async Task<VBForum> GetForumAsync(int forumId) {
            var forum = await db.Forums.FindAsync(forumId);
            return forum;
        }

        public Dictionary<VBForum, VBForumFlags> FetchForumsWithFlags(List<VBForum> forums, VBUserGroup userGroup) {
            var permissionDict = forums.Select(forum => {
                var highestPerms = FetchHighestPermissionFlagForUser(forum, userGroup);
                return new KeyValuePair<VBForum, VBForumFlags>(forum, highestPerms);
            }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return permissionDict;
        }

        VBForumFlags FetchHighestPermissionFlagForUser(VBForum forum, VBUserGroup userGroup) {
            // When there is no specific forum permission, it's inherited from the parent forum. 
            // Logic can be found in fetch_forum_permission function from includes/adminfunctions_forums.php
            var permission = GetHighestVBForumPermission(forum.Permissions, userGroup.Id);

            if (permission == null) {
                if (forum.Parent == null && forum.ParentId != -1) {
                    throw new Exception($"FetchHighestPermissionFlagForUser() needs Parent entity loaded for forum id {forum.Id}");
                }
                permission = GetHighestVBForumPermission(forum.Parent?.Permissions, userGroup.Id);
                if (permission == null) {
                    return userGroup.ForumPermissions;
                }
            }
            return permission.Flag;
        }

        VBForumPermission GetHighestVBForumPermission(List<VBForumPermission> forumPermissions, int userGroupId) {
            var permission = forumPermissions?.Where(p => p.UserGroupId == userGroupId)
                .OrderByDescending(perm => (int)perm.Flag)
                .FirstOrDefault();
            return permission;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ULabs.VBulletinEntity.LightModels;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightDashboardManager {
        readonly DbConnection db;
        public VBLightDashboardManager(VBDbContext db) {
            this.db = db.Database.GetDbConnection();
        }

        /// <summary>
        /// Gets a list of forum ids where the user group doesn't have at least view permission
        /// </summary>
        public async Task<List<int>> GetForumIdsWithoutViewPermissionAsync(int userGroupId) {
            var groupArgs = new { userGroupId };
            var groupPerm = await db.QueryFirstAsync<int>(@"
                SELECT forumpermissions
                FROM usergroup WHERE usergroupid = @userGroupId;", groupArgs);

            var args = new {
                userGroupId = userGroupId,
                groupPerm = groupPerm
            };
            var nonVisibleForumIds = await db.QueryAsync<int>(@"
                SELECT forum.forumid
                FROM forum
                LEFT JOIN forumpermission ON(forumpermission.forumid = forum.forumid AND (forumpermission.forumpermissions = null or forumpermission.usergroupid = @userGroupId))
                WHERE NOT (forumpermissions & 1) OR (forumpermissions IS NULL AND NOT (@groupPerm & 1));", args);
            return nonVisibleForumIds.ToList();
        }

        /// <summary>
        /// Fetches all categories (= forums without parents) with their corresponding lists of child forum ids
        /// </summary>
        public async Task<List<VBLightCategoryWithChilds>> GetCategoriesWithChildsAsyn() {
            var categoryChildLists = await db.QueryAsync<VBLightCategoryWithChilds>(@"
                SELECT forum.forumid as ForumId, forum.childlist as ChildsRaw
                FROM forum
                WHERE forum.parentid = -1");
            return categoryChildLists.ToList();
        }

        /// <summary>
        /// Gets the newest threads with some basic information aboud the forum and the user which wrote the last post
        /// </summary>
        /// <param name="count">Limit the fetched rows</param>
        /// <param name="forumIds">Optionally, you can pass a list of forum ids here to filter the threads</param>
        public async Task<List<VBLightThread>> GetNewestThreadsAsync(int count = 10, List<int> forumIds = null) {
            var args = new {
                childForumIds = forumIds,
                count = count
            };
            Func<VBLightThread, VBLightUser, VBLightForum, VBLightThread> mappingFunc = (thread, user, forum) => {
                thread.LastPoster = user;
                thread.Forum = forum;
                return thread;
            };
            var threads = await db.QueryAsync(@"
                    SELECT t.title as Title, t.threadid as ThreadId, t.lastpost as LastPostTimeRaw, t.lastposter as LastPosterName, t.lastposterid as LastPosterUserId, t.lastpostid as LastPostId, 
                        t.forumid as ForumId, t.replycount as ReplysCount,
                    u.userid as UserId, u.avatarrevision as AvatarRevision,
                    f.forumid as ForumId, f.title as Title
                    FROM thread t
                    LEFT JOIN user u ON (u.userid = t.lastposterid)
                    LEFT JOIN forum f ON (f.forumid = t.forumid)" +
                    (forumIds != null ? "WHERE t.forumid IN @childForumIds" : "") +
                    @"ORDER BY t.lastpost DESC
                    LIMIT @count", mappingFunc, args, splitOn: "LastPosterUserId,ForumId");
            return threads.ToList();
        }
    }
}

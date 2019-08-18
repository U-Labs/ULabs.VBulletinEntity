using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using ULabs.VBulletinEntity.LightModels;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightDashboardManager {
        readonly MySqlConnection db;
        public VBLightDashboardManager(MySqlConnection db) {
            this.db = db;
        }

        /// <summary>
        /// Gets a list of forum ids (key) with their corresponding childs (value) where the user group doesn't have at least view permission. The childs always contain the id of the parent forum (from VB DB)
        /// </summary>
        public Dictionary<int, List<int>> GetForumIdsWithoutViewPermission(int userGroupId) {
            var groupArgs = new { userGroupId };
            var groupPerm = db.QueryFirst<int>(@"
                SELECT forumpermissions
                FROM usergroup WHERE usergroupid = @userGroupId;", groupArgs);

            var args = new {
                userGroupId = userGroupId,
                groupPerm = groupPerm
            };
            var nonVisibleForumIds = db.Query(@"
                SELECT forum.forumid, forum.childlist
                FROM forum
                LEFT JOIN forumpermission ON(forumpermission.forumid = forum.forumid AND (forumpermission.forumpermissions = null or forumpermission.usergroupid = @userGroupId))
                WHERE NOT (forumpermissions & 1) OR (forumpermissions IS NULL AND NOT (@groupPerm & 1));", args);
            return nonVisibleForumIds.ToDictionary(
                x => (int)x.forumid, 
                x => ((string)x.childlist).Split(',')
                        .Select(int.Parse)
                        .ToList()
            );
        }

        /// <summary>
        /// Fetches all categories (= forums without parents) with their corresponding lists of child forum ids
        /// </summary>
        public List<VBLightCategoryWithChilds> GetCategoriesWithChilds() {
            var categoryChildLists = db.Query<VBLightCategoryWithChilds>(@"
                SELECT forum.forumid as ForumId, forum.childlist as ChildsRaw
                FROM forum
                WHERE forum.parentid = -1");
            return categoryChildLists.ToList();
        }

        /// <summary>
        /// Gets the newest threads with some basic information aboud the forum and the user which wrote the last post
        /// </summary>
        /// <param name="count">Limit the fetched rows</param>
        /// <param name="includedForumIds">Optionally, you can pass a list of forum ids here to filter the threads. Only includedForumIds or excludedForumIds can be specified at once.</param>
        /// <param name="excludedForumIds">Optionally list of forum ids to exclude. Only includedForumIds or excludedForumIds can be specified at once.</param>
        public List<VBLightThread> GetNewestThreads(int count = 10, List<int> includedForumIds = null, List<int> excludedForumIds = null) {
            if (includedForumIds != null && excludedForumIds != null) {
                throw new Exception("Both includedForumIds and excludedForumIds are specified, which doesn't make sense. Please remote one attribute from the GetNewestThreads() call.");
            }

            var args = new {
                includedForumIds = includedForumIds,
                excludedForumIds = excludedForumIds,
                count = count
            };
            Func<VBLightThread, VBLightUser, VBLightForum, VBLightThread> mappingFunc = (thread, user, forum) => {
                thread.LastPoster = user;
                thread.Forum = forum;
                return thread;
            };
            var threads = db.Query(@"
                    SELECT t.title as Title, t.threadid as ThreadId, t.lastpost as LastPostTimeRaw, t.lastposter as LastPosterName, t.lastposterid as LastPosterUserId, t.lastpostid as LastPostId, 
                        t.forumid as ForumId, t.replycount as ReplysCount,
                    u.userid as UserId, u.avatarrevision as AvatarRevision,
                    f.forumid as ForumId, f.title as Title
                    FROM thread t
                    LEFT JOIN user u ON (u.userid = t.lastposterid)
                    LEFT JOIN forum f ON (f.forumid = t.forumid) 
                    WHERE " +
                    (includedForumIds != null ? "t.forumid IN @includedForumIds " : "") +
                    (excludedForumIds != null ? "t.forumid NOT IN @excludedForumIds " : "") +
                    @"ORDER BY t.lastpost DESC
                    LIMIT @count", mappingFunc, args, splitOn: "LastPosterUserId,ForumId");
            return threads.ToList();
        }
    }
}

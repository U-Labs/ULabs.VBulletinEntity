using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULabs.VBulletinEntity.LightModels;
using ULabs.VBulletinEntity.LightModels.Forum;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Models.Permission;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightForumManager {
        readonly DbConnection db;

        public VBLightForumManager(MySqlConnection db) {
            this.db = db;
        }
        /// <summary>
        /// Gets a forum by its id
        /// </summary>
        public VBLightForum Get(int forumId) {
            string sql = @"
                SELECT f.forumid AS Id, f.title AS Title, f.parentid AS ParentId, f.parentlist AS ParentIdsRaw
                FROM forum f
                WHERE f.forumid = @forumId";
            var forum = db.QueryFirstOrDefault<VBLightForum>(sql, new { forumId });
            return forum;
        }

        /// <summary>
        /// Lists ALL child forums of the parent forums recursively on any nesting level. Used in our U-Labs Dashboard where we want to create category boxes based on existing forums with including any subforum.
        /// </summary>
        /// <param name="allForums">List of all existing forums to fetch childs from. Could be fetched using GetPermission when selecting the key.</param>
        /// <param name="parents">The forums which childs should get loaded</param>
        /// <returns></returns>
        public List<VBLightForum> GetChildsRecursive(List<VBLightForum> allForums, List<VBLightForum> parents) {
            var all = new List<VBLightForum>();
            parents.ForEach(parent => {
                // Childs of the first level from the current parent
                var currentSubs = allForums.Where(f => f.ParentId == parent.Id).ToList();
                bool subsExists = true;

                do {
                    all.AddRange(currentSubs);
                    currentSubs = allForums.Where(f => f.ParentIds.Any(p => currentSubs.Any(c => c.Id == p)))
                        .ToList();
                    subsExists = currentSubs.Any(c => !all.Any(a => a.Id == c.Id));
                } while (currentSubs != null && subsExists);
            });

            var uniqueChilds = all.Distinct().ToList();
            return uniqueChilds;
        }

        /// <summary>
        /// Get the VB bitfield permission for a given forum/usergroup. Checks the specific forum permission first and use group permission if forum permission doesnt exist.
        /// </summary>
        /// <param name="userGroupId">Id of the group to calculate permission for</param>
        /// <param name="forumId">Id of the forum that should be checked for access</param>
        public VBForumFlags GetPermission(int userGroupId, int forumId) {
            var args = new { userGroupId, forumId };
            string sql = @"
                SELECT IF(fp.forumpermissions IS NULL, g.forumpermissions, fp.forumpermissions)
                FROM forum f
                INNER JOIN usergroup g ON(g.usergroupid = @userGroupId)
                LEFT JOIN forumpermission fp ON(fp.usergroupid = g.usergroupid AND FIND_IN_SET(fp.forumid, f.parentlist))
                WHERE f.forumid = @forumId";
            int permission = db.QueryFirstOrDefault<int>(sql, args);
            return (VBForumFlags)Enum.Parse(typeof(VBForumFlags), permission.ToString());
        }

        /// <summary>
        /// Lists all forums with their corresponding permissions for the specified group
        /// </summary>
        /// <param name="userGroupId">Id of the group, where permissions should be fetched</param>
        /// <param name="onlyParentCategories">Per default, we fetch all forums. You can limit it to parent forums (parent = -1) by setting this to true.</param>
        /// <returns></returns>
        public Dictionary<VBLightForum, VBForumFlags> GetPermissions(int userGroupId, bool onlyParentCategories = false) {
            Func<VBLightForum, dynamic, KeyValuePair<VBLightForum, VBForumFlags>> mappingFunc = (forum, permissionRaw) => {
                string permissionValue = permissionRaw.Permission.ToString();
                var permission = (VBForumFlags)Enum.Parse(typeof(VBForumFlags), permissionValue);
                return new KeyValuePair<VBLightForum, VBForumFlags>(forum, permission);
            };
            // UserGroupId from permissions is only selected to have a split column for dapper. Instead we have to use parentlist, which needs mapping to VBLightForum by hand. 
            string sql = @"
                SELECT f.forumid AS Id, f.title AS Title, f.parentid AS ParentId, f.parentlist AS ParentIdsRaw,
                IF(fp.forumpermissions IS NULL, g.forumpermissions, fp.forumpermissions) AS Permission
                FROM forum f
                LEFT JOIN forumpermission fp ON(fp.usergroupid = @userGroupId AND FIND_IN_SET(fp.forumid, f.parentlist))
                INNER JOIN usergroup g ON(g.usergroupid = @userGroupId) " +
                (onlyParentCategories ? "AND f.parentid = -1" : "");

            var permissions = db.Query(sql, mappingFunc, new { userGroupId }, splitOn: "Permission")
                .ToDictionary(x => x.Key, x => x.Value);
            return permissions;
        }

        /// <summary>
        /// Returns a list of all forums where the group can do certain actions specified by flags
        /// </summary>
        public List<VBLightForum> GetForumsWhereUserCan(int userGroupId, VBForumFlags flags, bool onlyParentCategories = false) {
            return BuildForumPermissionQuery(userGroupId, flags, negate: false, onlyParentCategories);
        }

        /// <summary>
        /// Like <see cref="GetForumsWhereUserCan(int, VBForumFlags, bool)"/> but this method returns only the forum ids without any meta information
        /// </summary>
        public List<int> GetForumIdsWhereUserCan(int userGroupId, VBForumFlags flags, bool onlyParentCategories = false) {
            var forumIds = BuildForumPermissionQuery(userGroupId, flags, negate: true, onlyParentCategories, selectOnlyForumId: true)
                .Select(f => f.Id)
                .ToList();
            return forumIds;
        }

        /// <summary>
        /// Neogation of GetForumswhereUserCan(): It returns all forums where the group doesn't have the provided flag
        /// </summary>
        /// <param name="onlyParentCategories">If false, all forums will be fetched (default). Set this to true if you only want to get categories (parent = -1).</param>
        public List<VBLightForum> GetForumsWhereUserCanNot(int userGroupId, VBForumFlags flags, bool onlyParentCategories = false) {
            return BuildForumPermissionQuery(userGroupId, flags, negate: true, onlyParentCategories);
        }

        List<VBLightForum> BuildForumPermissionQuery(int userGroupId, VBForumFlags? flags, bool negate, bool onlyParentCategories, bool selectOnlyForumId = false) {
            var args = new {
                userGroupId,
                flags = (int)flags
            };
            bool hasWhere = flags != null || onlyParentCategories;

            var sql = new StringBuilder("SELECT f.forumid AS Id");
            if(!selectOnlyForumId) {
                sql.Append(", f.title AS Title, f.parentid AS ParentId, f.parentlist AS ParentIdsRaw");
            }

            sql.Append(@"
                FROM forum f
                INNER JOIN usergroup g ON(g.usergroupid = @userGroupId)
                LEFT JOIN forumpermission fp ON(fp.usergroupid = g.usergroupid AND FIND_IN_SET(fp.forumid, f.parentlist))");

            if (hasWhere) {
                sql.Append("WHERE ");
            }
            if(flags != null) {
                sql.Append((negate ? "NOT " : "") + "(IF(fp.forumpermissions IS NULL, g.forumpermissions, fp.forumpermissions) & @flags) ");
            }
            if(onlyParentCategories) {
                sql.Append("AND f.parentid = -1 ");
            }

            sql.Append("GROUP BY f.forumid;");
            var forums = db.Query<VBLightForum>(sql.ToString(), args);
            return forums.ToList();
        }

        public PageContentInfo GetForumThreadsInfo(List<int> forumIds, int page = 1, int threadsPerPage = 20) {
            int offset = (page - 1) * threadsPerPage;
            var info = new PageContentInfo(page, threadsPerPage);

            string sqlThreadIds = $@"
                SELECT t.threadid
                FROM thread t
                WHERE t.forumid IN @forumIds
                AND t.visible = 1 
                ORDER BY t.lastpost DESC
                LIMIT @offset, @threadsPerPage";
            var sqlThreadArgs = new { forumIds, offset, threadsPerPage };
            info.ContentIds = db.Query<int>(sqlThreadIds, sqlThreadArgs).ToList();

            var totalPagesArgs = new { forumIds, threadsPerPage };
            string sqlTotalPages = @"
                SELECT CEIL(COUNT(*) / @threadsPerPage) AS pages
                FROM thread
                WHERE visible = 1
                AND forumId IN @forumIds";
            info.TotalPages = db.QueryFirstOrDefault<int>(sqlTotalPages, totalPagesArgs);

            return info;
        }
        public PageContentInfo GetForumThreadsInfo(int forumId, int page = 1, int threadsPerPage = 20) {
            return GetForumThreadsInfo(new List<int>() { forumId }, page, threadsPerPage);
        }
        public List<VBLightForumThread> GetForumThreads(PageContentInfo info) {
            string sql = @"
                SELECT threadid AS Id, title, open, replycount AS ReplysCount, dateline AS CreatedTimeRaw, postusername AS AuthorUserName, postuserid AS AuthorUserId, lastposter AS lastPosterUserName, 
                    lastposterid AS lastPosterUserId, views AS ViewsCount
                FROM thread 
                WHERE threadid IN @ContentIds
                ORDER BY lastpost DESC;";
            return db.Query<VBLightForumThread>(sql, new { info.ContentIds }).ToList();
        }
    }
}

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
                SELECT f.forumid AS ForumId, f.title AS Title, f.parentid AS ParentId, f.parentlist AS ParentIdsRaw
                FROM forum f
                WHERE f.forumid = @forumId";
            var forum = db.QueryFirstOrDefault<VBLightForum>(sql, new { forumId });
            return forum;
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
        /// Returns a list of all forums where the group can do certain actions specified by flags
        /// </summary>
        public List<VBLightForum> GetForumsWhereUserCan(int userGroupId, VBForumFlags flags, bool onlyParentCategories = false) {
            return BuildForumPermissionQuery(userGroupId, flags, negate: false, onlyParentCategories);
        }

        /// <summary>
        /// Neogation of GetForumswhereUserCan(): It returns all forums where the group doesn't have the provided flag
        /// </summary>
        /// <param name="onlyParentCategories">If false, all forums will be fetched (default). Set this to true if you only want to get categories (parent = -1).</param>
        public List<VBLightForum> GetForumsWhereUserCanNot(int userGroupId, VBForumFlags flags, bool onlyParentCategories = false) {
            return BuildForumPermissionQuery(userGroupId, flags, negate: true, onlyParentCategories);
        }

        List<VBLightForum> BuildForumPermissionQuery(int userGroupId, VBForumFlags? flags, bool negate, bool onlyParentCategories) {
            var args = new {
                userGroupId,
                flags = (int)flags
            };
            bool hasWhere = flags != null || onlyParentCategories;

            var sql = new StringBuilder();
            sql.Append(@"
                SELECT f.forumid AS ForumId, f.title AS Title, f.parentid AS ParentId, f.parentlist AS ParentIdsRaw
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
    }
}

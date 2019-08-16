using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
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
        public List<int> GetForumIdsWithoutViewPermission(int userGroupId) {
            var groupArgs = new { userGroupId };
            var groupPerm = db.Query<int>(@"
                SELECT forumpermissions
                FROM usergroup WHERE usergroupid = @userGroupId;", groupArgs)
                .FirstOrDefault();

            var args = new {
                userGroupId = userGroupId,
                groupPerm = groupPerm
            };
            var nonVisibleForumIds = db.Query<int>(@"
                SELECT forum.forumid
                FROM forum
                LEFT JOIN forumpermission ON(forumpermission.forumid = forum.forumid AND (forumpermission.forumpermissions = null or forumpermission.usergroupid = @userGroupId))
                WHERE NOT (forumpermissions & 1) OR (forumpermissions IS NULL AND NOT (@groupPerm & 1));", args);
            return nonVisibleForumIds.ToList();
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
    }
}

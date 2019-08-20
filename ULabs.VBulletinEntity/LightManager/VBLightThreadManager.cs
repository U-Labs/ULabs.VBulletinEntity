using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ULabs.VBulletinEntity.LightModels.Thread;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightThreadManager {
        readonly MySqlConnection db;
        public VBLightThreadManager(MySqlConnection db) {
            this.db = db;
        }
        /// <summary>
        /// Gets received thanks from other users for the posts of the specified user id from "Post Thank you Hack" addon
        /// </summary>
        /// <param name="userId">Id of the user that we should query for received thanks</param>
        /// <param name="afterTimestamp">If specified, only thanks after this timestamp are returned (optional)</param>
        /// <param name="count">Limit the number of thanks to return. Recommended since older/larger boards can return a massive amount of data if no limit is specified.</param>
        /// <returns></returns>
        public List<VBLightPostThanks> GetThanks(int userId, int? afterTimestamp = null, int count = 10) {
            string sql = @"
                SELECT pt.date AS TimeRaw, pt.postid AS PostId,
			        t.threadid AS ThreadId, t.title AS ThreadTitle,
			        f.forumid AS ForumId, f.title AS ForumTitle
                FROM post_thanks AS pt
                LEFT JOIN post AS p ON (p.postid = pt.postid)
                LEFT JOIN thread AS t ON (t.threadid = p.threadid)
                LEFT JOIN forum f ON(f.forumid = t.forumid)
                WHERE p.userid = @userId ";
            if(afterTimestamp.HasValue) {
                sql += "AND pt.date > @afterTimestamp";
            }
            sql += @"
                ORDER BY pt.id DESC
                LIMIT @count";

            var thanks = db.Query<VBLightPostThanks>(sql, new { userId, afterTimestamp, count });
            return thanks.ToList();
        }
    }
}

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
using ULabs.VBulletinEntity.Caching;
using ULabs.VBulletinEntity.LightModels;
using ULabs.VBulletinEntity.LightModels.Session;
using ULabs.VBulletinEntity.LightModels.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightUserManager {
        readonly DbConnection db;
        readonly IVBCache cache;
        // No SELECT included for the flexibility to use this in complex queries. HasAvatar is a join to the custom avatar table, the only method to make 100% sure that the avatar exist. 
        // It covers the case where an user had an avatar (revision > 0) and deleted those avatar. We keep the column by the user without a join for dapper, since it's only a single field
        internal string UserColumnSql = @"
            u.userid as Id, u.username AS UserName, u.usertitle AS UserTitle, u.lastactivity AS LastActivityRaw, u.avatarrevision AS AvatarRevision, u.pmunread AS UnreadPmsCount, 
                u.recent_thankcnt AS UnreadThanksCount,
            c.filename IS NOT NULL AS HasAvatar,
            g.usergroupid as Id, g.opentag as OpenTag, g.closetag as CloseTag, g.usertitle as UserTitle, g.adminpermissions as AdminPermissions";
        Func<VBLightUser, VBLightUserGroup, VBLightUser> mappingFunc = (dbUser, group) => {
            dbUser.PrimaryUserGroup = group;
            return dbUser;
        };
        public VBLightUserManager(MySqlConnection db, IVBCache cache) {
            this.db = db;
            this.cache=cache;
        }
        ~VBLightUserManager() {
            db.Close();
        }

        List<VBLightPrivateMessage> GetPrivateMessages(int userId, string additionalConditions, VBPrivateMessageReadState? readState = null, int count = 10, int? textPreviewWords = null) {
            Func<VBLightPrivateMessage, VBLightUser, VBLightUserGroup, VBLightPrivateMessage> mappingFunc = (pm, user, group) => {
                pm.FromUser = user;
                pm.FromUser.PrimaryUserGroup = group;
                return pm;
            };
            string textSelectColumn = (textPreviewWords.HasValue ? "SUBSTRING_INDEX(txt.message, ' ', @textPreviewWords)" : "txt.message");
            // folder -1 is the outgoing mail folder. The filter avoids that we get messages send by the current user
            string sql = $@"
                SELECT pm.pmid, pm.pmtextid, pm.parentpmid, pm.messageread AS MessageReadRaw, 
                {textSelectColumn} AS text, txt.title, txt.dateline AS SendTimeRaw, txt.fromuserid AS Id,
                u.username AS UserName, u.usertitle AS UserTitle, u.lastactivity AS LastActivityRaw, u.avatarrevision AS AvatarRevision, c.filename IS NOT NULL AS HasAvatar,
                            g.usergroupid as Id, g.opentag as OpenTag, g.closetag as CloseTag, g.usertitle as UserTitle, g.adminpermissions as AdminPermissions 
                FROM pm, pmtext AS txt
                LEFT JOIN user u ON(u.userid = txt.fromuserid)
                LEFT JOIN usergroup g ON(g.usergroupid = u.usergroupid)
                LEFT JOIN customavatar c ON(c.userid = u.userid)
                WHERE pm.pmtextid = txt.pmtextid 
                {additionalConditions}
                AND pm.userid = @userId " +
                (readState != null ? "AND pm.messageread = @readStateRaw " : "") + @"
                ORDER BY txt.dateline DESC
                LIMIT @count";

            int readStateRaw = (readState.HasValue ? (int)readState.Value : 0);
            var args = new { userId, readStateRaw, textPreviewWords, count };
            var pms = db.Query(sql, mappingFunc, args);
            return pms.ToList();
        }

        List<VBLightUser> UserQuery(string sqlConditions, object param = null) {
            string sql = $@"
                SELECT {UserColumnSql}
                FROM user u
                LEFT JOIN usergroup g ON(g.usergroupid = u.usergroupid)
                LEFT JOIN customavatar c ON(c.userid = u.userid)
                {sqlConditions}";
            var users = db.Query(sql, mappingFunc, param);
            return users.ToList();
        }
        public VBLightUser Get(int userId) {
            var user = UserQuery("WHERE u.userid = @userId", new { userId });
            return user.FirstOrDefault();
        }

        /// <summary>
        /// Loads private messages that were send to <paramref name="userId"/>
        /// </summary>
        /// <param name="readState">Filter the messages to read/unread or answered messages</param>
        /// <param name="count">Maximum amount of messages that would be returned</param>
        /// <param name="textPreviewWords">If set, the messages Text property will be an excerpt to this amound of workds. Setting to null gives the full content</param>
        public List<VBLightPrivateMessage> GetReceivedPrivateMessages(int userId, VBPrivateMessageReadState? readState = null, int count = 10, int? textPreviewWords = null) {
            return GetPrivateMessages(userId, "AND pm.folderid != -1", readState, count, textPreviewWords);
        }

        /// <summary>
        /// Same as <see cref="GetReceivedPrivateMessages(int, VBPrivateMessageReadState?, int, int?)"/> but counts only the matching pms instead of fetching their data
        /// </summary>
        public int CountUnreadPrivateMessages(int userId, VBPrivateMessageReadState? readState = null) {
            string sql = @"
                SELECT COUNT(*)
                FROM pm
                WHERE pm.userid = @userId 
                AND pm.folderid != -1 " +
                (readState != null ? "AND pm.messageread = @readStateRaw " : "");

            int readStateRaw = (readState.HasValue ? (int)readState.Value : 0);
            int count = db.QueryFirstOrDefault<int>(sql, new { userId, readStateRaw });
            return count;
        }

        /// <summary>
        /// Fetches the unread thanks counter from the users table to avoid getting cached information from the session (better as purging the more complex query there)
        /// </summary>
        public int UnreadThanksCount(int userId) {
            int count = db.QueryFirstOrDefault<int>("SELECT recent_thankcnt FROM user WHERE userid = @userId", new { userId });
            return count;
        }
        /// <summary>
        /// Reset the unviewed recent thanks from the Post thank hack addon in the userinfo if the user has read the notification about them. 
        /// Session hash is used to purge the session cache if enabled.
        /// </summary>
        public void ResetRecentThanks(int userId, string sessionHash) {
            string sql = "UPDATE user SET recent_thankcnt = 0 WHERE userid = @userId";
            db.Execute(sql, new { userId });

            // The session references to the user entity where the old thanks state is still stored
            cache.Remove(VBCacheKey.LightSession, sessionHash);
        }

        public List<VBLightUser> GetUsersByLastActivity(DateTime lastActivityLimit) {
            long lastActivityTs = lastActivityLimit.ForceUtc().ToUnixTimestamp();
            string sqlCondition = @"
                WHERE u.lastactivity >= @lastActivityTs
                ORDER BY u.lastactivity DESC";
            var users = UserQuery(sqlCondition, new { lastActivityTs });
            return users;
        }
    }
}

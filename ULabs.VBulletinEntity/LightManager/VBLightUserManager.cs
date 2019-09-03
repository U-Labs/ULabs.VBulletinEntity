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
using ULabs.VBulletinEntity.LightModels.Session;
using ULabs.VBulletinEntity.LightModels.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightUserManager {
        readonly DbConnection db;
        // No SELECT included for the flexibility to use this in complex queries
        internal string UserColumnSql = @"
            u.username AS UserName, u.usertitle AS UserTitle, u.lastactivity AS LastActivityRaw, u.avatarrevision AS AvatarRevision, u.pmunread AS UnreadPmsCount,
            g.usergroupid as Id, g.opentag as OpenTag, g.closetag as CloseTag, g.usertitle as UserTitle, g.adminpermissions as AdminPermissions ";
        public VBLightUserManager(MySqlConnection db) {
            this.db = db;
        }
        ~VBLightUserManager() {
            db.Close();
        }

        public VBLightUser Get(int userId) {
            Func<VBLightUser, VBLightUserGroup, VBLightUser> mappingFunc = (dbUser, group) => {
                dbUser.PrimaryUserGroup = group;
                return dbUser;
            };
            string sql = $@"
                SELECT {UserColumnSql}
                FROM user u
                LEFT JOIN usergroup g ON(g.usergroupid = u.usergroupid)
                WHERE u.userid = @userId";
            var user = db.Query(sql, mappingFunc, new { userId });
            return user.FirstOrDefault();
        }
        /// <summary>
        /// Loads private messages that were send to <paramref name="userId"/>
        /// </summary>
        /// <param name="readState">Filter the messages to read/unread or answered messages</param>
        /// <param name="count">Maximum amount of messages that would be returned</param>
        /// <param name="textPreviewWords">If set, the messages Text property will be an excerpt to this amound of workds. Setting to null gives the full content</param>
        /// <returns></returns>
        public List<VBLightPrivateMessage> GetPrivateMessages(int userId, VBPrivateMessageReadState? readState = null, int count = 10, int? textPreviewWords = null) {
            Func<VBLightPrivateMessage, VBLightUser, VBLightUserGroup, VBLightPrivateMessage> mappingFunc = (pm, user, group) => {
                pm.FromUser = user;
                pm.FromUser.PrimaryUserGroup = group;
                return pm;
            };
            string textSelectColumn = (textPreviewWords.HasValue ? "SUBSTRING_INDEX(txt.message, ' ', @textPreviewWords)" : "txt.message");
            string sql = $@"
                SELECT pm.pmid, pm.pmtextid, pm.parentpmid, pm.messageread AS MessageReadRaw, 
                {textSelectColumn} AS text, txt.title, txt.dateline AS SendTimeRaw, txt.fromuserid AS Id,
                u.username AS UserName, u.usertitle AS UserTitle, u.lastactivity AS LastActivityRaw, u.avatarrevision AS AvatarRevision, 
                            g.usergroupid as Id, g.opentag as OpenTag, g.closetag as CloseTag, g.usertitle as UserTitle, g.adminpermissions as AdminPermissions 
                FROM pm, pmtext AS txt
                LEFT JOIN user u ON(u.userid = txt.fromuserid)
                LEFT JOIN usergroup g ON(g.usergroupid = u.usergroupid)
                WHERE pm.pmtextid = txt.pmtextid
                AND pm.userid = @userId " +
                (readState != null ? "AND pm.messageread = @readStateRaw " : "") + @"
                ORDER BY txt.dateline DESC
                LIMIT @count";

            int readStateRaw = (readState.HasValue ? (int)readState.Value : 0);
            var args = new { userId, readStateRaw, textPreviewWords, count };
            var pms = db.Query(sql, mappingFunc, args);
            return pms.ToList();
        }
    }
}

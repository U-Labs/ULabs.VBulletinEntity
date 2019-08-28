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
        readonly IHttpContextAccessor contextAccessor;
        readonly DbConnection db;
        // No SELECT included for the flexibility to use this in complex queries
        internal string UserColumnSql = @"
            u.username AS UserName, u.usertitle AS UserTitle, u.lastactivity AS LastActivityRaw, u.avatarrevision AS AvatarRevision, 
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

    }
}

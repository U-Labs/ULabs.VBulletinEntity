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
using ULabs.VBulletinEntity.Shared.Caching;
using ULabs.VBulletinEntity.LightModels;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightSettingsManager {
        readonly DbConnection db;
        readonly IVBCache cache;
        VBLightCommonSettings commonSettings;

        public VBLightSettingsManager(MySqlConnection db, IVBCache cache) {
            this.db = db;
            this.cache = cache;
        }
        
        public VBLightCommonSettings CommonSettings {
            get {
                if(commonSettings == null) {
                    if(cache.TryGet(VBCacheKey.LightCommonSettings, out VBLightCommonSettings settings)) {
                        commonSettings = settings;
                        return commonSettings;
                    }

                    string sql = @"
                        SELECT varname, value
                        FROM setting
                        WHERE varname IN('bburl', 'recycle_forum', 'maxposts', 'cookiedomain')";
                    var rawSettings = db.Query(sql).ToDictionary(
                        row => (string)row.varname,
                        row => (string)row.value
                    );;
                    commonSettings = new VBLightCommonSettings(rawSettings);
                    cache.Set(VBCacheKey.LightCommonSettings, commonSettings, TimeSpan.FromDays(7));
                }
                return commonSettings;
            }
        }
    }
}

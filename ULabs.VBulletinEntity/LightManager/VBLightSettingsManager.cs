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
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightSettingsManager {
        readonly DbConnection db;
        VBLightCommonSettings commonSettings;

        public VBLightSettingsManager(MySqlConnection db) {
            this.db = db;
        }
        
        public VBLightCommonSettings CommonSettings {
            get {
                if(commonSettings == null) {
                    string sql = @"
                        SELECT varname, value
                        FROM setting
                        WHERE varname IN('bburl')";
                    var rawSettings = db.Query(sql).ToDictionary(
                        row => (string)row.varname,
                        row => (string)row.value
                    );;
                    commonSettings = new VBLightCommonSettings(rawSettings);
                }
                return commonSettings;
            }
        }
    }
}

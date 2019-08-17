using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels {
    public class VBLightCommonSettings {
        public string BaseUrl { get; set; }

        public VBLightCommonSettings(Dictionary<string,string> sqlData) {
            BaseUrl = sqlData["bburl"];
        }
    }
}

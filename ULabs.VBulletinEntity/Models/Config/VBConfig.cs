using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Config {
    /// <summary>
    /// Configuration options not specified in the database
    /// </summary>
    public class VBConfig {
        /// <summary>
        /// Defined in ./includes/functions.php:34
        /// </summary>
        public string CookieSalt { get; set; }
        /// <summary>
        /// config.php: $config['Misc']['cookieprefix'] - the default value is bb_
        /// </summary>
        public string CookiePrefix { get; set; }

        public VBConfig(string cookieSalt, string cookiePrefix = "bb_") {
            CookieSalt = cookieSalt;
            CookiePrefix = cookiePrefix;
        }
    }
}

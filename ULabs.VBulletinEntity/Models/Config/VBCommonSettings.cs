using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Config {
    /// <summary>
    /// In contrast to other models, Column is not optional here for attributes that exists in a db, even if they were named the same. [Column] is used to filter those attributes
    /// for mapping them in VBSettingsManager.LoadCommonSettings() since we want to use clean as possible OOP on different settings in the same table
    /// </summary>
    public class VBCommonSettings {
        // ToDo: Extend the model with more settings (and maybe additional types on top of string/int in VBSettingsManager)
        [Column("bburl")]
        public string BoardUrl { get; set; }

        [Column("cookietimeout")]
        public int CookieTimeoutRaw { get; set; }

        public TimeSpan CookieTimeout {
            get { return TimeSpan.FromSeconds(CookieTimeoutRaw); }
            set { CookieTimeoutRaw = (int)value.TotalSeconds; }
        }

        [Column("bbtitle")]
        public string ProjectTitle { get; set; }

        [Column("usestrikesystem")]
        public string UseStrikeSystem { get; set; }

        [Column("avatarurl")]
        public string CustomAvatarsFolder { get; set; }

        // ToDo: Path to attachments (attachpath in version group) is avaliable here, so we don't neet it in custom settings
    }
}

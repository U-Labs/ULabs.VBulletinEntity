using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Config {
    /// <summary>
    /// In contrast to other models, Column is not optional here for attributes that exists in a db, even if they were named the same. [Column] is used to filter those attributes
    /// for mapping them in VBSettingsManager.LoadCommonSettings() since we want to use clean as possible OOP on different settings in the same table
    /// </summary>
    public class VBCommonSettings {
        [Column("bbtitle")]
        public string ProjectTitle { get; set; }

        /// <summary>
        /// Base URL for the entire vBulletin installation
        /// </summary>
        [Column("bburl")]
        public string BaseUrl { get; set; }

        /// <summary>
        /// If enabled, the base URL is used to generate links in vB. Otherwise, the currently used URL acts as base. 
        /// </summary>
        [Column("bburl_basepath")]
        public bool UseVBulletinUrlAsBaseUrl { get; set; }

        /// <summary>
        /// Optional URL for the forum. If specified, the forum will use this url instead of BoardUrl
        /// </summary>
        [Column("vbforum_url")]
        public string ForumUrl { get; set; }

        /// <summary>
        /// Optional Base-URL for the CMS
        /// </summary>
        [Column("vbcms_url")]
        public string CmsBaseUrl { get; set; }

        /// <summary>
        /// Optional Base-URL for the VB Blogs
        /// </summary>
        [Column("vbblog_url")]
        public string BlogBaseUrl { get; set; }

        [Column("redirect_whitelist")]
        public string RedirectWhitelistUrlsRaw { get; set; }

        [Column("redirect_whitelist_disable")]
        public bool RedirectWhitelistDisabled { get; set; }

        [Column("bbmenu")]
        public bool ShowForumLinkInMenu { get; set; }

        [Column("cookietimeout")]
        public int CookieTimeoutRaw { get; set; }

        [Column("hometitle")]
        public string HomepageName { get; set; }

        [Column("homeurl")]
        public string HomepageUrl { get; set; }

        [Column("contactuslink")]
        public string ContactUsLink { get; set; }

        [Column("contactustype")]
        public bool ShowContactUsLinkForGuests { get; set; }

        [Column("usestrikesystem")]
        public string UseStrikeSystem { get; set; }

        [Column("contactusoptions")]
        public string ContactUsSubjectsRaw { get; set; }

        [Column("contactusother")]
        public bool ContactUsAllowCustomSubject { get; set; }

        public string WebmasterEmail { get; set; }

        /// <summary>
        /// VB will send all emails created by the contact form to this address
        /// </summary>
        [Column("contactusemail")]
        public string ContactTargetMail { get; set; }
        public string PrivacyUrl { get; set; }
        public string CopyrightText { get; set; }
        public string CompanyName { get; set; }
        public string TosUrl { get; set; }
        public string Faxnumber { get; set; }

        [Column("address")]
        public string OwnerAddress { get; set; }

        [NotMapped]
        public List<string> ContactUsSubjects {
            get => ContactUsSubjectsRaw.Split('\n').ToList();
            set => ContactUsSubjectsRaw = string.Join("\n", value);
        }

        [NotMapped]
        public List<string> RedirectWhitelistUrls {
            get => RedirectWhitelistUrlsRaw.Split('\n').ToList();
            set => RedirectWhitelistUrlsRaw = string.Join("\n", value);
        }

        [NotMapped]
        public TimeSpan CookieTimeout {
            get => TimeSpan.FromSeconds(CookieTimeoutRaw);
            set => CookieTimeoutRaw = (int)value.TotalSeconds;
        }
    }
}

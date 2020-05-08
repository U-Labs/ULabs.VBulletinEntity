using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ULabs.VBulletinEntity.Shared.Tools {
    public class ContentTools {
        const int maxSeoTitleLength = 150;
        static readonly Dictionary<string, string> replacedChars = new Dictionary<string, string>() {
            // Only lowercase chars 
            ["ä"] = "ae",
            ["ö"] = "oe",
            ["ü"] = "ue",
            ["ß"] = "ss"
        };

        public static string SeoTitle(string title) {
            // Can be null if we have VBThread in ViewModel and we only transfer it's Id using hidden fields
            if (title == null) {
                return "";
            }

            title = WebUtility.HtmlDecode(title);
            string lowerTitle = title.ToLower();
            replacedChars.ToList().ForEach(kvp =>
                lowerTitle = lowerTitle.Replace(kvp.Key, kvp.Value)
            );

            string seoTitle = Regex.Replace(lowerTitle, "[^a-z0-9.\\-]+", "-", RegexOptions.Compiled);
            if (seoTitle.Length > maxSeoTitleLength) {
                return seoTitle.Substring(0, maxSeoTitleLength);
            }

            seoTitle = seoTitle.Replace("--", "")
                .Trim('-');
            return seoTitle;
        }

        /// <summary>
        /// Converts lowercasestring to LowerCaseString. Usefull for mapping VB lowercase variables to C#s PascalCase, e.g. "desc" to "Desc".
        /// </summary>
        public static string ToPascalCase(string str) {
            if (!string.IsNullOrEmpty(str) && str.Length > 1) {
                return char.ToUpperInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }
    }
}

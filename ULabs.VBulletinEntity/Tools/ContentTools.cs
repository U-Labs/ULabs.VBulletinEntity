using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ULabs.VBulletinEntity.Tools {
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
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ULabs.VBulletinEntity.Caching;
using ULabs.VBulletinEntity.Models.Config;
using ULabs.VBulletinEntity.Models.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Manager {
    public class VBSessionManager {
        readonly VBDbContext db;
        readonly VBUserManager userManager;
        readonly VBSettingsManager settingsManager;
        readonly VBConfig vbConfig;
        readonly IHttpContextAccessor contextAccessor;
        readonly IVBCache cache;
        VBSession currentSession;
        const string sessionHashCookieName = "sessionhash";
        const int guestUserGroupId = 1;

        public VBSessionManager(VBDbContext db, VBUserManager userManager, VBSettingsManager settingsManager, VBConfig vbConfig, IHttpContextAccessor contextAccessor, 
            IVBCache cache) {

            this.db = db;
            this.userManager = userManager;
            this.settingsManager = settingsManager;
            this.vbConfig = vbConfig;
            this.contextAccessor = contextAccessor;
            this.cache = cache;
        }

        public async Task<VBSession> GetAsync(string sessionHash, bool updateLastActivity = false) {
            if (!updateLastActivity && cache.TryGet(VBCacheKey.UserSession, sessionHash, out VBSession session)) {
                return session;
            }

            int cookieTimeoutSpan = GetCookieTimeoutSpam();
            session = await db.Sessions.Include(s => s.User)
                .ThenInclude(u => u.UserGroup)
                .Include(s => s.User)
                .ThenInclude(u => u.CustomAvatar)
                .SingleOrDefaultAsync(s => s.SessionHash == sessionHash && s.LastActivityRaw > cookieTimeoutSpan);

            if (session != null && updateLastActivity) {
                session.LastActivity = DateTime.UtcNow;
                session.Location = contextAccessor.HttpContext.Request.Path;
                await db.SaveChangesAsync();
            }

            cache.Set(VBCacheKey.UserSession, sessionHash, session, settingsManager.GetCommonSettings().CookieTimeout);
            return session;
        }

        public async Task<VBSession> GetCurrentAsync() {
            if (currentSession == null) {
                // ToDo: Consistent using the new prefix in ManualForumSettings instead of guessing the cookie
                string sessionCookie = GetCookieWithoutPrefix(sessionHashCookieName);
                currentSession = await GetAsync(sessionCookie, updateLastActivity: true);

                if (currentSession == null) {
                    var cookieUser = GetUserFromCookiePasswordAsync().Result;
                    // No session detected: User must be a guest
                    if (cookieUser == null) {
                        currentSession = await CreateGuestSessionAsync();
                        int cnt = db.SaveChanges();
                        return currentSession;
                    }

                    // Cleanup old sessions that were invalid due to cookie timeout
                    // ToDo: Document raw query
                    int cookieTimeoutSpan = GetCookieTimeoutSpam();
                    int deletedOldSessionsCount = await db.Database.ExecuteSqlCommandAsync($"delete from session where userid = {cookieUser.Id} and lastactivity < {cookieTimeoutSpan}");

                    currentSession = await CreateAsync(cookieUser, location: contextAccessor.HttpContext.Request.Path);

                    // ToDo: Set cookie for TLD when using subdomains (default is current subdomain if used)
                    var context = contextAccessor.HttpContext;
                    var cookieOptions = new CookieOptions() {
                        Expires = DateTime.Now.Add(settingsManager.GetCommonSettings().CookieTimeout)
                    };
                    context.Response.Cookies.Append($"{vbConfig.CookiePrefix}{sessionHashCookieName}", currentSession.SessionHash, cookieOptions);

                    return currentSession;
                }
            }
            return currentSession;
        }

        public VBSession GetCurrent() {
            return GetCurrentAsync().Result;
        }

        int GetCookieTimeoutSpam() {
            int currrentTs = DateTime.UtcNow.ToUnixTimestampAsInt();
            // vB_Session constructor 
            int cookieTimeoutSpan = currrentTs - settingsManager.GetCommonSettings().CookieTimeoutRaw;
            return cookieTimeoutSpan;
        }

        string GetCookieNameWithoutPrefix(string name) {
            var cookies = contextAccessor.HttpContext.Request.Cookies;
            var cookieName = cookies.Keys.ToList()
                .FirstOrDefault(c => c.EndsWith(name));
            return cookieName;
        }

        string GetCookieWithoutPrefix(string name) {
            string sessionCookieName = GetCookieNameWithoutPrefix(name);
            if (sessionCookieName == default(string)) {
                return null;
            }

            var cookieValue = contextAccessor.HttpContext.Request.Cookies[sessionCookieName];
            if (cookieValue == default(string)) {
                return null;
            }
            return cookieValue;
        }

        async Task<VBUser> GetUserFromCookiePasswordAsync() {
            string cookiePassword = GetCookieWithoutPrefix("password");
            string rawCookieUserId = GetCookieWithoutPrefix("userid");
            int cookieUserId;
            if (!int.TryParse(rawCookieUserId, out cookieUserId)) {
                return null;
            }

            var cookieUser = await userManager.GetUserAsync(cookieUserId);
            if (cookieUser == null) {
                return null;
            }

            string verifiedPasswordHash = Hash.Md5($"{cookieUser.Password}{vbConfig.CookieSalt}");
            return verifiedPasswordHash == cookiePassword ? cookieUser : null;
        }

        async Task<VBSession> CreateGuestSessionAsync() {
            var user = new VBUser() {
                UserGroupId = guestUserGroupId
            };
            user.UserGroup = await db.UserGroups.FindAsync(guestUserGroupId);
            // ToDo: Add session attributes like location
            var session = await CreateAsync(user);
            // Tracking for guest sessions is tricky and dangerous: We change the user attribute to a non existing one, which cause exceptions on save - so EF shouldn track them
            db.Entry(session).State = EntityState.Detached;
            // CreateAsync only assign the UserId so that we don't insert dummy guest users here by those realtion
            session.User = user;
            return session;
        }

        public async Task<VBSession> CreateAsync(VBUser user, int inForumId = 0, int inThreadId = 0, string location = "", int styleId = 0, int languageId = 0, bool saveChanges = true) {
            var context = contextAccessor.HttpContext;
            var session = new VBSession() {
                SessionHash = GenerateHash(),
                IdHash = GenerateIdHash(),
                IsBot = false,
                UserId = user.Id,
                UserAgent = GetUserAgent(),
                LoggedInRaw = user.Id > 0 ? 1 : 0,
                InForumId = inForumId,
                InThreadId = inThreadId,
                Location = location,
                StyleId = styleId,
                LanguageId = languageId,
                Host = GetClientIpAddress().ToString(),
                LastActivity = DateTime.UtcNow
            };
            await CreateAsync(session);
            return session;
        }

        public async Task<VBSession> CreateAsync(VBSession session, bool saveChanges = true) {
            await db.Sessions.AddAsync(session);
            if (saveChanges) {
                await db.SaveChangesAsync();
            }
            return session;
        }

        string GenerateHash() {
            // 32 Characters long and more random than the original fetch_sessionhash() method from vB in includes/class_core.php
            return Guid.NewGuid().ToString("N");
        }

        string GenerateIdHash() {
            // define('SESSION_IDHASH', md5($_SERVER['HTTP_USER_AGENT'] . $this->fetch_substr_ip($this->getIp())));
            string ipBeginning = GetClientIpAddress().ToString();
            ipBeginning = ipBeginning.Substring(0, ipBeginning.LastIndexOf('.'));

            string idHash = Hash.Md5($"{GetUserAgent()}{ipBeginning}");
            return idHash;
        }

        string GetUserAgent() {
            var requestHeaders = contextAccessor.HttpContext.Request.Headers;
            if (requestHeaders.ContainsKey(HeaderNames.UserAgent)) {
                return requestHeaders[HeaderNames.UserAgent];
            }
            return "";
        }

        // We need this also VBThreadManager for creating replys
        public IPAddress GetClientIpAddress() {
            // ToDo: Test if https://stackoverflow.com/a/41335701/3276634 works remote and secure it for prod env (accept those headers only from trusted ips/networks)
            return contextAccessor.HttpContext.Connection.RemoteIpAddress;
        }
    }
}

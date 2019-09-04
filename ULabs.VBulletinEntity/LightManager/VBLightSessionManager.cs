using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULabs.VBulletinEntity.Caching;
using ULabs.VBulletinEntity.LightModels;
using ULabs.VBulletinEntity.LightModels.Session;
using ULabs.VBulletinEntity.LightModels.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightSessionManager {
        readonly IHttpContextAccessor contextAccessor;
        readonly DbConnection db;
        readonly string cookieSalt;
        readonly VBSessionHelper sessionHelper;
        readonly VBLightSettingsManager lightSettingsManager;
        readonly VBLightUserManager lightUserManager;
        readonly IVBCache cache;
        IRequestCookieCollection contextCookies;
        TimeSpan cookieTimeout = new TimeSpan(days: 30, 0, 0, 0);
        readonly ILogger logger;
        VBLightSession currentSession;

        public VBLightSessionManager(IHttpContextAccessor contextAccessor, VBSessionHelper sessionHelper, VBLightSettingsManager lightSettingsManager, VBLightUserManager lightUserManager,
            MySqlConnection db, ILogger<VBLightSessionManager> logger, IVBCache cache, string cookieSalt, string cookiePrefix) {
            this.contextAccessor = contextAccessor;
            this.sessionHelper = sessionHelper;
            this.lightSettingsManager = lightSettingsManager;
            this.lightUserManager = lightUserManager;
            this.db = db;
            CookiePrefix = cookiePrefix;
            this.cookieSalt = cookieSalt;
            this.logger = logger;
            this.cache = cache;

            // For the case that we are not in a  http request
            logger.LogDebug($"VBLightSessionManager HttpContext exists: {contextAccessor.HttpContext != null} - CacheProvider: {cache.GetType().Name}");
            if (contextAccessor.HttpContext != null) {
                contextCookies = contextAccessor.HttpContext.Request.Cookies;
            }
        }

        #region Private methods
        /// <summary>
        /// Allows other systems like SignalR to fetch the cookie prefix for custo, integrations
        /// </summary>
        public string CookiePrefix { get; private set; }
        ~VBLightSessionManager() {
            db.Close();
        }
        int? GetCookieUserId(IRequestCookieCollection cookies) {
            string cookieUserIdRaw = cookies[$"{CookiePrefix}_userid"];
            if (int.TryParse(cookieUserIdRaw, out int cookieUserId)) {
                return cookieUserId;
            }
            return null;
        }
        bool ValidateUserFromCookie(IRequestCookieCollection cookies, int cookieUserId) {
            var cookiePassword = cookies[$"{CookiePrefix}_password"];

            string userDbPassword = db.QueryFirstOrDefault<string>("SELECT password FROM user WHERE userid = @userId", new { userId = cookieUserId });
            if (string.IsNullOrEmpty(userDbPassword)) {
                // The password is invalid. Now we don't have any chance to validate the users credentials
                return false;
            }

            string verifiedPasswordHash = Hash.Md5($"{userDbPassword}{cookieSalt}");
            return verifiedPasswordHash == cookiePassword;
        }
        string GetCurrentLocation(string location = "") {
            if (!string.IsNullOrEmpty(location)) {
                return location;
            }

            if (contextAccessor != null) {
                var request = contextAccessor.HttpContext.Request;
                location = $"{request.PathBase}{request.Path}";
            }

            return location;
        }
        string CensoreHash(string sessionHash) {
            string censoredHash = $"{sessionHash.Substring(0, sessionHash.Length - 10)}-{new String('X', 10)}";
            return censoredHash;
        }
        #endregion

        public VBLightSession Get(string sessionHash) {
            if(cache.TryGet(VBCacheKey.LightSession, sessionHash, out VBLightSession session)) {
                logger.LogDebug($"Get: Return session for hash = {CensoreHash(sessionHash)} from CACHE");
                return session;
            }

            logger.LogDebug($"No cache avaliable for session with hash = {CensoreHash(sessionHash)}, fetch from database");
            Func<VBLightSession, VBLightUser, VBLightUserGroup, VBLightSession> mappingFunc = (dbSession, user, group) => {
                // No related user and so no group exists on guest sessions
                if (group != null) {
                    user.PrimaryUserGroup = group;
                    dbSession.User = user;
                } else {
                    dbSession.User = new VBLightUser() {
                        PrimaryUserGroup = new VBLightUserGroup() {
                            Id = 2
                        }
                    };
                }

                return dbSession;
            };
            // ToDo: Validate Cookie timeout 
            // LightThreadManager uses parts of this query for fetchinng the author user with its group
            string sql = $@"
                SELECT s.sessionhash AS SessionHash, s.idhash AS IdHash, s.lastactivity AS LastActivityRaw, s.location AS location, s.useragent AS UserAgent, 
                        s.loggedin AS LoggedInRaw, s.isbot AS IsBot, s.userid AS Id, 
                    {lightUserManager.UserColumnSql}
                FROM session s
                LEFT JOIN user u ON (u.userid = s.userid)
                LEFT JOIN usergroup g ON(g.usergroupid = u.usergroupid)
                WHERE s.sessionhash = @sessionHash
                LIMIT 1";
            var args = new { sessionHash = sessionHash };
            session = db.Query(sql, mappingFunc, args)
                .SingleOrDefault();

            if (session != null) {
                // ToDo: Set TTL based on lifetime of sessions specified in VB
                cache.Set(VBCacheKey.LightSession, session.SessionHash, session, TimeSpan.FromMinutes(30));
            }
            return session;
        }
        public VBLightSession GetCurrent(IRequestCookieCollection cookies = null) {
            if (currentSession != null) {
                logger.LogDebug($"Return session by current session singleton for user #{currentSession.User.Id}");
                return currentSession;
            }

            cookies = cookies ?? contextAccessor.HttpContext.Request.Cookies;
            if (cookies.TryGetValue($"{CookiePrefix}_sessionhash", out string sessionHash)) {
                currentSession = Get(sessionHash);
                if (currentSession != null) {
                    logger.LogDebug($"Sessionash belongs to a valid session from user {currentSession.User.Id}, return session object");
                    return currentSession;
                }
            }

            // Attemp 2: We don't have a valid VB session, but the users pw may be still stored in the cookie which we can use to generate a new session as some kind of SSO
            // ToDo: Set location, delete old invalid sessions
            int? cookieUserId = GetCookieUserId(cookies);
            string location = GetCurrentLocation();
            logger.LogDebug($"GetCurrent: Validate user from cookie. UserId = {(cookieUserId.HasValue ? cookieUserId.Value : 0)}");

            if (cookieUserId.HasValue && cookieUserId.Value > 0 && ValidateUserFromCookie(cookies, cookieUserId.Value)) {
                logger.LogDebug($"GetCurrent: Refresh password-validated authorized session for #{cookieUserId.Value} with location {location}");
                sessionHash = Create(cookieUserId.Value, "");
            } else {
                logger.LogDebug($"GetCurrent: Create guest session for location = {location} with hash = {sessionHash}");
                sessionHash = Create(0, location);
            }

            // ToDo: Could improve performance so that we dont fetch the guest session here again
            currentSession = Get(sessionHash);
            return currentSession;
        }

        public void Update(HttpContext context, string location = "", string sessionHash = "") {
            if (!string.IsNullOrEmpty(location)) {
                location = context.Request.Path;
            }
            if (string.IsNullOrEmpty(sessionHash)) {
                var session = GetCurrent();
                sessionHash = session.SessionHash;
            }

            UpdateLastActivity(sessionHash, location);
            UpdateSessionCookie(context, sessionHash);
        }
        public void UpdateSessionCookie(HttpContext context, string sessionHash) {
            string cookieName = $"{CookiePrefix}_sessionhash";
            var cookieValue = context.Request.Cookies[cookieName];
            if (cookieValue == sessionHash) {
                return;
            }

            var cookieOptions = new CookieOptions() {
                // ToDo: Set session duration from config like VBSessionManager does
                Expires = DateTime.Now.Add(cookieTimeout)
            };

            string cookieDomain = lightSettingsManager.CommonSettings.CookieDomain;
            if (!string.IsNullOrEmpty(cookieDomain)) {
                cookieOptions.Domain = cookieDomain;
            }

            logger.LogDebug($"Update session cookie with domain {cookieDomain}");
            context.Response.Cookies.Append(cookieName, sessionHash, cookieOptions);
        }

        public void UpdateLastActivity(string sessionHash, string location) {
            logger.LogInformation($"Update sessionhash = {CensoreHash(sessionHash)} with location = {location}");

            var args = new { sessionHash, location };
            db.Execute(@"
                UPDATE session
                SET lastactivity = UNIX_TIMESTAMP(),
                location = @location
                WHERE sessionhash = @sessionHash;

                UPDATE user
                SET lastactivity = UNIX_TIMESTAMP()
                WHERE userid = (SELECT userid FROM session WHERE sessionhash = @sessionHash);
            ", args);
        }
        public string Create(int userId, string location, int inForumId = 0, int inThreadId = 0) {
            // ToDo: Add forum/thread for location
            string sql = @"
                INSERT into session
                SET sessionhash = @sessionHash,
                userid = @userId,
                host = @ip,
                idhash = @idHash,
                lastactivity = UNIX_TIMESTAMP(),
                location = @location,
                useragent = @userAgent,
                styleid = 0,
                languageid = 0,
                loggedin = @loggedIn,
                inforum = @inForumId,
                inthread = @inThreadId";

            var args = new {
                sessionHash = sessionHelper.GenerateHash(),
                idHash = sessionHelper.GenerateIdHash(),
                userId = userId,
                location = location,
                userAgent = sessionHelper.GetUserAgent(),
                ip = sessionHelper.GetClientIpAddress(),
                inForumId = inForumId,
                inThreadId = inThreadId,
                loggedIn = userId > 0 ? 1 : 0
            };
            db.Execute(sql, args);
            return args.sessionHash;
        }

        public string GetAvatarUrl(int? userId, int? avatarRevision) {
            if (!userId.HasValue || !avatarRevision.HasValue || avatarRevision == 0) {
                // ToDo: VB has not setting for the default Avatar. We should specify this in custom settings somewhere
                return "https://u-img.net/img/4037Ld.png";
            }
            return $"{lightSettingsManager.CommonSettings.BaseUrl}/customavatars/avatar{userId}_{avatarRevision}.gif";
        }
    }
}

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
    public class VBLightSessionManager {
        readonly IHttpContextAccessor contextAccessor;
        readonly DbConnection db;
        readonly string cookieSalt;
        readonly VBSessionHelper sessionHelper;
        readonly VBLightSettingsManager lightSettingsManager;
        IRequestCookieCollection contextCookies;
        TimeSpan cookieTimeout = new TimeSpan(days: 30, 0, 0, 0);

        public VBLightSessionManager(IHttpContextAccessor contextAccessor, VBSessionHelper sessionHelper, VBLightSettingsManager lightSettingsManager, MySqlConnection db, string cookieSalt, string cookiePrefix) {
            this.contextAccessor = contextAccessor;
            this.sessionHelper = sessionHelper;
            this.lightSettingsManager = lightSettingsManager;
            this.db = db;
            CookiePrefix = cookiePrefix;
            this.cookieSalt = cookieSalt;
            contextCookies = contextAccessor.HttpContext.Request.Cookies;
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

            string userDbPassword = db.QueryFirst<string>("SELECT password FROM user WHERE userid = @userId", new { userId = cookieUserId });
            if (string.IsNullOrEmpty(userDbPassword)) {
                // The password is invalid. Now we don't have any chance to validate the users credentials
                return false;
            }

            string verifiedPasswordHash = Hash.Md5($"{userDbPassword}{cookieSalt}");
            return verifiedPasswordHash == cookiePassword;
        }
        void SetSessionCookie(string sessionHash) {
            var context = contextAccessor.HttpContext;
            var cookieOptions = new CookieOptions() {
                Expires = DateTime.Now.Add(cookieTimeout)
            };
            context.Response.Cookies.Append($"{CookiePrefix}_sessionhash", sessionHash, cookieOptions);
        }
        string GetCurrentLocation(string location) {
            if (!string.IsNullOrEmpty(location)) {
                return location;
            }

            if (contextAccessor != null) {
                var request = contextAccessor.HttpContext.Request;
                location = $"{request.PathBase}{request.Path}";
            }

            return location;
        }
        #endregion
        public VBLightSession GetCurrent(IRequestCookieCollection cookies = null, bool createIfRestoreable = true, string location = "", bool updateLastActivity = false) {
            if (cookies == null) {
                cookies = contextCookies;
            }
            VBLightSession session = null;
            location = GetCurrentLocation(location);

            if (cookies.TryGetValue($"{CookiePrefix}_sessionhash", out string sessionHash)) {
                session = Get(sessionHash, updateLastActivity, location);

                if (session != null) {
                    if (updateLastActivity) {
                        UpdateLastActivity(sessionHash, location);
                    }
                    return session;
                }
            }

            if (createIfRestoreable) {
                // Attemp 2: We don't have a valid VB session, but the users pw may be still stored in the cookie which we can use to generate a new session as some kind of SSO
                // ToDo: Set location, delete old invalid sessions
                int? cookieUserId = GetCookieUserId(cookies);
                if (cookieUserId.HasValue && ValidateUserFromCookie(cookies, cookieUserId.Value)) {
                    sessionHash = Create(cookieUserId.Value, location);
                } else {
                    sessionHash = Create(0, location);
                }

                session = Get(sessionHash, updateLastActivity, location);
                // ToDo: Set session duration from config like VBSessionManager does
                SetSessionCookie(session.SessionHash);
            }
            return session;
        }

        public VBLightSession Get(string sessionHash, bool updateLastActivity = false, string location = "") {
            Func<VBLightSession, VBLightUser, VBLightSession> mappingFunc = (dbSession, user) => {
                dbSession.User = user;
                return dbSession;
            };
            // ToDo: Validate Cookie timeout 
            string sql = @"
                SELECT s.sessionhash AS SessionHash, s.userid AS UserId, s.idhash AS IdHash, s.lastactivity AS LastActivityRaw, s.location AS location, s.useragent AS UserAgent, 
                        s.loggedin AS LoggedInRaw, s.isbot AS IsBot,
                    u.userid AS UserId, u.usergroupid AS PrimaryUserGroupId, u.username AS UserName, u.usertitle AS UserTitle, u.lastactivity AS LastActivityRaw, u.avatarrevision AS AvatarRevision
                FROM session s
                LEFT JOIN user u ON (u.userid = s.userid)
                WHERE s.sessionhash = @sessionHash
                LIMIT 1";
            var args = new { sessionHash = sessionHash };
            var session = db.Query(sql, mappingFunc, args, splitOn: "UserId")
                .SingleOrDefault();

            if (session != null && updateLastActivity) {
                UpdateLastActivity(session.SessionHash, location);
            }
            return session;
        }

        public void UpdateLastActivity(string sessionHash, string location) {
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

        public string GetAvatarUrl(int userId, int avatarRevision) {
            if (avatarRevision == 0) {
                // ToDo: VB has not setting for the default Avatar. We should specify this in custom settings somewhere
                return "https://u-img.net/img/4037Ld.png";
            }
            return $"{lightSettingsManager.CommonSettings.BaseUrl}/customavatars/avatar{userId}_{avatarRevision}.gif";
        }
    }
}

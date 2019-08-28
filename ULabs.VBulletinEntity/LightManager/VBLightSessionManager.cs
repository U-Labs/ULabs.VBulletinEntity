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
        IRequestCookieCollection contextCookies;
        TimeSpan cookieTimeout = new TimeSpan(days: 30, 0, 0, 0);
        // No SELECT included for the flexibility to use this in complex queries
        string userColumnSql = @"
            u.username AS UserName, u.usertitle AS UserTitle, u.lastactivity AS LastActivityRaw, u.avatarrevision AS AvatarRevision, 
            g.usergroupid as Id, g.opentag as OpenTag, g.closetag as CloseTag, g.usertitle as UserTitle, g.adminpermissions as AdminPermissions ";
        public VBLightSessionManager(IHttpContextAccessor contextAccessor, VBSessionHelper sessionHelper, VBLightSettingsManager lightSettingsManager, MySqlConnection db, string cookieSalt, string cookiePrefix) {
            this.contextAccessor = contextAccessor;
            this.sessionHelper = sessionHelper;
            this.lightSettingsManager = lightSettingsManager;
            this.db = db;
            CookiePrefix = cookiePrefix;
            this.cookieSalt = cookieSalt;
            
            // For the case that we are not in a  http request
            if(contextAccessor.HttpContext != null) {
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

        /// <summary>
        /// Fetches the current user session from the injected IHttpContextAccessor instance or a custom cookie collection
        /// </summary>
        /// <param name="cookies">If not null, the cookies are used for session fetching. Otherwise the HttpContext of the injected IHttpContextAccessor instance</param>
        /// <param name="restore">We only use the sessionhash cookie to fetch sessions if set to true. Otherweise, the method try to fetch/create also new sessions using pw hash/userid cookies</param>
        /// <param name="saveRestored"><paramref name="restore"/> is true, this determinates if we save a new generated session from restore (true) or create a pseudo session with user details (false).
        /// Setting it to false can be usefull if used in a context where cookies can't be send (e.g. SignalR WebSocket application). 
        /// </param>
        /// <param name="location">Location to set/update on the session. Is not set on restored session if <paramref name="restore"/>if set to false</param>
        /// <param name="updateLastActivity">Updates the sessions and users last activity if a session exists</param>
        /// <returns>The user session or in case of <paramref name="restore"/> = true and <paramref name="saveRestored"/> = false a pseudo-session with User/Loggedin set to true.</returns>
        public VBLightSession GetCurrent(IRequestCookieCollection cookies = null, bool restore = true, bool saveRestored = true, string location = "", bool updateLastActivity = false) {
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

            if (restore) {
                // Attemp 2: We don't have a valid VB session, but the users pw may be still stored in the cookie which we can use to generate a new session as some kind of SSO
                // ToDo: Set location, delete old invalid sessions
                int? cookieUserId = GetCookieUserId(cookies);
                if (cookieUserId.HasValue && cookieUserId.Value > 0 && ValidateUserFromCookie(cookies, cookieUserId.Value)) {
                    if(!saveRestored) {
                        // This is a pseudo session based on the users id and password that we verfied
                        session = new VBLightSession() {
                            User = GetUser(cookieUserId.Value),
                            LoggedInRaw = 2
                        };
                        return session;
                    }

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
            Func<VBLightSession, VBLightUser, VBLightUserGroup, VBLightSession> mappingFunc = (dbSession, user, group) => {
                // No related user and so no group exists on guest sessions
                if(group != null) {
                    user.PrimaryUserGroup = group;
                    dbSession.User = user;
                }else {
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
                    {userColumnSql}
                FROM session s
                LEFT JOIN user u ON (u.userid = s.userid)
                LEFT JOIN usergroup g ON(g.usergroupid = u.usergroupid)
                WHERE s.sessionhash = @sessionHash
                LIMIT 1";
            var args = new { sessionHash = sessionHash };
            var session = db.Query(sql, mappingFunc, args)
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
        VBLightUser GetUser(int userId) {
            Func<VBLightUser, VBLightUserGroup, VBLightUser> mappingFunc = (dbUser, group) => {
                dbUser.PrimaryUserGroup = group;
                return dbUser;
            };
            string sql = $@"
                SELECT {userColumnSql}
                FROM user u
                LEFT JOIN usergroup g ON(g.usergroupid = u.usergroupid)
                WHERE u.userid = @userId";
            var user = db.Query(sql, mappingFunc, new { userId });
            return user.FirstOrDefault();
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

using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ULabs.VBulletinEntity.LightModels;
using ULabs.VBulletinEntity.LightModels.Forum;
using ULabs.VBulletinEntity.LightModels.User;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Models.Manager;
using ULabs.VBulletinEntity.Models.Permission;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightThreadManager {
        #region Private attributes and methods
        readonly MySqlConnection db;
        readonly VBLightForumManager lightForumManager;

        // Order (also with Id and splitOn set): All attributes from the first relation entity should be placed BEFORE the (SplitOn) key
        string threadBaseQuery = @"
            SELECT t.threadid as Id, t.title as Title, t.lastpost as LastPostTimeRaw, t.lastpostid as LastPostId, t.firstpostid as FirstPostId,
                        t.replycount as ReplysCount, t.deletedcount as DeletedReplysCount, t.open as IsOpen, t.lastposterid as LastPosterUserId, t.postuserid as AuthorUserId, t.visible as IsVisible,
                    u.userid as Id, u.avatarrevision as AvatarRevision, u.username as UserName, u.usertitle as UserTitle, u.lastactivity as LastActivityRaw,
                    f.forumid as Id, f.title as Title,
                    g.usergroupid as Id, g.opentag as OpenTag, g.closetag as CloseTag, g.usertitle as UserTitle, g.adminpermissions as AdminPermissions
                FROM thread t
                LEFT JOIN user u ON (u.userid = t.lastposterid)
                LEFT JOIN forum f ON (f.forumid = t.forumid)
                LEFT JOIN usergroup g ON (g.usergroupid = u.usergroupid) ";
        string postBaseQuery = @"
                SELECT p.postid AS Id, p.threadid AS ThreadId, p.parentid AS ParentPostId, p.dateline AS CreatedTimeRaw, p.pagetext AS TEXT, p.ipaddress AS IpAddress, p.visible AS VisibilityRaw, 
                        p.attach AS HasAttachments, p.post_thanks_amount AS ThanksCount,
                    u.userid AS Id, u.username AS UserName, u.usertitle AS UserTitle, u.avatarrevision AS AvatarRevision, u.lastactivity AS LastActivityRaw,
                    g.usergroupid AS Id, g.opentag AS OpenTag, g.closetag AS CloseTag, g.usertitle AS UserTitle, g.adminpermissions AS AdminPermissions 
                FROM post p
                LEFT JOIN user u ON (u.userid = p.userid)
                LEFT JOIN usergroup g ON (u.usergroupid = g.usergroupid) ";
        Func<VBLightThread, VBLightUser, VBLightForum, VBLightUserGroup, VBLightThread> threadMappingFunc = (thread, user, forum, group) => {
            thread.LastPoster = user;
            thread.Forum = forum;
            thread.LastPoster.PrimaryUserGroup = group;
            return thread;
        };
        Func<VBLightPost, VBLightUser, VBLightUserGroup, VBLightPost> postMappingFunc = (post, author, group) => {
            if (author != null) {
                post.Author = author;
                post.Author.PrimaryUserGroup = group;
            }
            return post;
        };
        #endregion
        public VBLightThreadManager(MySqlConnection db, VBLightForumManager lightForumManager) {
            this.db = db;
            this.lightForumManager = lightForumManager;
        }

        public VBLightThread Get(int threadId) {
            var args = new { threadId };
            string sql = threadBaseQuery + @"WHERE t.threadid = @threadId";
            // Generic overload not possible with QueryFirstOrDefault()
            var threads = db.Query(sql, threadMappingFunc, args);
            return threads.SingleOrDefault();
        }

        /// <summary>
        /// Loads the replys of a thread for the page, which meta data were fetched by <see cref="VBLightThreadManager.GetReplysInfo(int, bool, int, int)"/>. 
        /// </summary>
        /// <param name="replysInfo"></param>
        /// <returns></returns>
        public List<VBLightPost> GetReplys(ReplysInfo replysInfo) {
            string sql = $@"
                {postBaseQuery}
                WHERE p.postid IN @postIds
                ORDER BY p.dateline";
            var replys = db.Query(sql, postMappingFunc, new { postIds = replysInfo.PostIds });
            return replys.ToList();
        }

        /// <summary>
        /// Fetches meta information about the thread replys which are required to calculate paging
        /// </summary>
        /// <param name="includeDeleted">Include deleted posts for moderators or admin users</param>
        /// <param name="page">Number of the page to display, which is used for calculating which posts to skip</param>
        /// <param name="replysPerPage">How much replys should be present on a single page. VBulletins default is 10.</param>
        public ReplysInfo GetReplysInfo(int threadId, bool includeDeleted = false, int page = 1, int replysPerPage = 10) {
            int offset = (page - 1) * replysPerPage;
            var args = new { threadId, offset, replysPerPage };
            var info = new ReplysInfo(page, replysPerPage);

            string sqlWithoutSelect = @"
                FROM post p 
                WHERE p.threadId = @threadId " +
                (includeDeleted ? "" : "AND p.visible = 1 ") + @"
                ORDER BY p.dateline";

            string sqlPostIds = $@"
                SELECT p.postid 
                {sqlWithoutSelect}
                LIMIT @offset, @replysPerPage";
            info.PostIds = db.Query<int>(sqlPostIds, args)
                .ToList();

            // Cant calculate the total pages by Math.Ceiling((decimal)info.PostIds.Count / (decimal)replysPerPage); because we need all post ids for this = Bad performance on large threads
            string sqlTotalPages = $"SELECT CEIL(COUNT(p.postid) / @replysPerPage) {sqlWithoutSelect}";
            info.TotalPages = db.QueryFirstOrDefault<int>(sqlTotalPages, args);
            return info;
        }

        /// <summary>
        /// Calculates the page of a specific post reply
        /// </summary>
        public int GetPageOfReply(int threadId, int replyId, int replysPerPage = 10, bool includeDeleted = false) {
            var args = new { threadId, replyId, replysPerPage };
            // With the dateline we get 100% correct order. PostId should be working too in most cases, but not when posts were inserted with newer timestamp (e.g. import from other forum)
            // Fetching the entire post list can slow down the performance a bit on large threads (not much, < 100ms). This approach is faster by letting the sql server do the work
            string sql = @"
 		        SELECT CEIL(COUNT(p.postid) / @replysPerPage)
                FROM post p 
                WHERE p.threadId = @threadId " +
                (includeDeleted ? "" : "AND p.visible = 1 ") + @"
                AND p.dateline <= (SELECT dateline FROM post WHERE postid = @replyId)
                ORDER BY p.dateline";
            int page = db.QuerySingleOrDefault<int>(sql, args);
            return page;
        }

        /// <summary>
        /// Gets a single post. Usefull when a user wants to edit its post.
        /// </summary>
        public VBLightPost GetPost(int postId) {
            string sql = $@"
                {postBaseQuery}
                WHERE p.postid = @postId";
            var reply = db.Query(sql, postMappingFunc, new { postId });
            return reply.FirstOrDefault();
        }

        /// <summary>
        /// Gets the newest threads with some basic information aboud the forum and the user which wrote the last post
        /// </summary>
        /// <param name="count">Limit the fetched rows</param>
        /// <param name="onlyWithoutReplys">If true, only new threads without any reply will be fetched (good to display new threads without any reply)</param>
        /// <param name="orderByLastPostDate">If true, the threads were ordered by the date of their last reply. Otherwise the creation time of the thread is used.</param>
        /// <param name="includedForumIds">Optionally, you can pass a list of forum ids here to filter the threads. Only includedForumIds or excludedForumIds can be specified at once.</param>
        /// <param name="excludedForumIds">Optionally list of forum ids to exclude. Only includedForumIds or excludedForumIds can be specified at once.</param>
        public List<VBLightThread> GetNewestThreads(int count = 10, bool onlyWithoutReplys = false, bool orderByLastPostDate = false, List<int> includedForumIds = null, List<int> excludedForumIds = null) {
            if (includedForumIds != null && excludedForumIds != null) {
                throw new Exception("Both includedForumIds and excludedForumIds are specified, which doesn't make sense. Please remote one attribute from the GetNewestThreads() call.");
            }

            var args = new { includedForumIds, excludedForumIds, count = count };
            bool hasExclude = excludedForumIds != null || includedForumIds != null;
            bool hasWhere = hasExclude || onlyWithoutReplys;
            string sql = threadBaseQuery +
                    (hasWhere ? "WHERE " : "") +
                    (includedForumIds != null ? "t.forumid IN @includedForumIds " : "") +
                    (excludedForumIds != null ? "t.forumid NOT IN @excludedForumIds " : "") +
                    (onlyWithoutReplys ? (hasExclude ? "AND " : "") + "t.replycount = 0 " : "") +
                    @"ORDER BY " + (orderByLastPostDate ? "t.lastpost " : "t.dateline ") + @"DESC
                    LIMIT @count";
            var threads = db.Query(sql, threadMappingFunc, args);
            return threads.ToList();
        }

        /// <summary>
        /// Lists Threads with new replys from others in which the user was active (wrote at lest one post) ordered by last post time of the thread.
        /// Note that you should disable auto deletion of the contentread in VB to use this! Otherwise users will get already seen new replys to older threads because of the contentread table purge!
        /// </summary>
        /// <param name="userId">Id of the user to check for new replys in his active threads</param>
        /// <param name="count">Limit the amount of entries. Recommended since active users may get a larger set of data</param>
        /// <param name="ignoredForumIds">Don't fetch notifications if they were posted in those forum ids </param>
        public List<VBLightUnreadActiveThread> GetUnreadActiveThreads(int userId, int count = 10, List<int> ignoredForumIds = null) {
            var args = new { userId, count, ignoredForumIds };

            // Grouping by contentid (which is the thread id) avoid returning a row for each post the user made in this thread
            // ContentId 2 = Threads
            string sql = @"
                SELECT r.contentid AS ThreadId, 
					t.title AS ThreadTitle, t.lastpost AS LastPostTimeRaw,
				    f.forumid AS ForumId, f.title AS ForumTitle,
					u.userid AS LastPosterUserId, u.avatarrevision AS LastPosterAvatarRevision
                FROM contentread r, post p, thread t, forum f, user u
                WHERE r.contenttypeid = 2
                AND r.readtype = 'view'
                AND r.contentid = p.threadid
                AND t.threadid = r.contentid
                AND p.userid = r.userid
                AND t.lastpost > r.dateline
                AND r.userid = @userId
                AND t.lastposterid != r.userid 
                AND t.forumid = f.forumid
                AND u.userid = t.lastposterid " +
                (ignoredForumIds != null ? "AND t.forumid NOT IN @ignoredForumIds " : "") + @"
                GROUP BY r.contentid
                ORDER BY t.lastpost DESC
                LIMIT @count";
            var unreadThreads = db.Query<VBLightUnreadActiveThread>(sql, args);
            return unreadThreads.ToList();
        }

        /// <summary>
        /// Updates the contentread table to mark VB content as read or inserts a new row for completely unread threads
        /// </summary>
        /// <param name="contentId">Id of the VB content (e.g. ThreadId)</param>
        /// <param name="userId">Id of the user who read the content</param>
        /// <param name="contentTypeId">2 for threads</param>
        /// <param name="readType">VB enum: read, view or other</param>
        public void MarkContentAsRead(int contentId, int userId, int contentTypeId = 2, string readType = "view") {
            var args = new { contentId, userId, contentTypeId, readType };
            string sql = @"
                INSERT INTO contentread(contenttypeid, contentid, userid, readtype, dateline)
                    VALUES (@contentTypeId, @contentId, @userId, @readType, UNIX_TIMESTAMP())
                ON DUPLICATE KEY UPDATE dateline = UNIX_TIMESTAMP()
            ";
            db.Execute(sql, args);
        }

        /// <summary>
        /// Gets received thanks from other users for the posts of the specified user id from "Post Thank you Hack" addon.
        /// For best performance, it's recommended to add an index (the table doesn't have any except on own id): ALTER TABLE post_thanks ADD INDEX userid_date(userid, DATE);
        /// </summary>
        /// <param name="userId">Id of the user that we should query for received thanks</param>
        /// <param name="afterTimestamp">If specified, only thanks after this timestamp are returned (optional)</param>
        /// <param name="count">Limit the number of thanks to return. Recommended since older/larger boards can return a massive amount of data if no limit is specified.</param>
        public List<VBLightPostThanks> GetThanks(int userId, int? afterTimestamp = null, int count = 10) {
            string sql = @"
                SELECT pt.date AS TimeRaw, pt.postid AS PostId,
			        t.threadid AS ThreadId, t.title AS ThreadTitle,
			        f.forumid AS ForumId, f.title AS ForumTitle
                FROM post_thanks AS pt
                LEFT JOIN post AS p ON (p.postid = pt.postid)
                LEFT JOIN thread AS t ON (t.threadid = p.threadid)
                LEFT JOIN forum f ON(f.forumid = t.forumid)
                WHERE pt.userid = @userId ";
            if (afterTimestamp.HasValue) {
                sql += "AND pt.date > @afterTimestamp";
            }
            sql += @"
                ORDER BY pt.id DESC
                LIMIT @count";

            var thanks = db.Query<VBLightPostThanks>(sql, new { userId, afterTimestamp, count });
            return thanks.ToList();
        }

        /// <summary>
        /// Get the list of post ids out of <paramref name="postIds"/> on which <paramref name="userId"/> has already thanked
        /// </summary>
        public List<int> GetPostsWhereUserThanked(int userId, List<int> postIds) {
            string sql = @"
                SELECT pt.postid
                FROM post_thanks pt
                WHERE pt.userid = @userId
                AND pt.postid IN @postIds";
            var thankedPostIds = db.Query<int>(sql, new { userId, postIds });
            return thankedPostIds.ToList();
        }

        /// <summary>
        /// Checks if a SEO url provided by MVC arguments matches the full generated url of the thread (forum with thread). All variables prefixed with "received" are from the method arguments.
        /// </summary>
        public bool SeoUrlMatch(VBLightThread thread, string receivedForumTitle, int receivedForumId, string receivedThreadTitle, int receivedThreadId) {
            string generated = $"{thread.Forum.SeoUrlPart}/{thread.SeoUrlPart}";
            string received = $"{receivedForumTitle}-{receivedForumId}/{receivedThreadTitle}-{receivedThreadId}";
            return received == generated;
        }

        /// <summary>
        /// Checks if the specified user has proper permission to reply on threads in the forum
        /// </summary>
        /// <example>
        /// <code>
        /// var model = new LightCreateReplyModel(lightSessionManager.GetCurrent().User, forumId: 123, threadId: 456, text: "Testreply", ipAddress: "127.0.0.1");
        /// var check = lightThreadManager.CreateReplyCheck(model);
        /// </code>
        /// </example>
        /// <param name="thread"><see cref="VBLightThread"></see> of <paramref name="replyModel"/> ThreadId. Optional to save one query in combination with 
        /// <see cref="VBLightThreadManager.CreateReply(LightCreateReplyModel)"</param>
        public CanReplyResult CreateReplyCheck(LightCreateReplyModel replyModel, VBLightThread thread = null) {
            if (thread == null) {
                thread = Get(replyModel.ThreadId);
            }

            if (thread == null || !thread.IsVisible) {
                return CanReplyResult.ThreadNotExisting;
                // ToDo: Test mod check in full thread manager and test if we need to implement it there too  
            } else if (!thread.IsOpen && !replyModel.Author.PrimaryUserGroup.AdminPermissions.HasFlag(VBAdminFlags.IsModerator)) {
                return CanReplyResult.ThreadClosed;
            }

            var createOtherOrMyThreadsFlag = thread.AuthorUserId == replyModel.Author.Id ? VBForumFlags.CanReplyToOwnThreads : VBForumFlags.CanReplyToOtherThreads;
            var forumPermission = lightForumManager.GetPermission(replyModel.Author.PrimaryUserGroup.Id, replyModel.ForumId);
            if(!forumPermission.HasFlag(createOtherOrMyThreadsFlag)) {
                return CanReplyResult.NoReplyPermission;
            }
            return CanReplyResult.Ok;
        }
    }
}

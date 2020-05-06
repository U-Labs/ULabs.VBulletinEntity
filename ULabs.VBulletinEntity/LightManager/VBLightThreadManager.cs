using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ULabs.VBulletinEntity.LightModels;
using ULabs.VBulletinEntity.LightModels.Forum;
using ULabs.VBulletinEntity.LightModels.Moderation;
using ULabs.VBulletinEntity.LightModels.User;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Models.Manager;
using ULabs.VBulletinEntity.Models.Permission;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.LightManager {
    public class VBLightThreadManager {
        #region Private attributes and methods
        readonly MySqlConnection db;
        readonly VBLightForumManager lightForumManager;

        // Order (also with Id and splitOn set): All attributes from the first relation entity should be placed BEFORE the (SplitOn) key
        string threadBaseQuery = @"
            SELECT t.threadid as Id, t.title as Title, t.dateline AS CreatedTimeRaw, t.lastpost as LastPostTimeRaw, t.lastpostid as LastPostId, t.firstpostid as FirstPostId,
                        t.replycount as ReplysCount, t.deletedcount as DeletedReplysCount, t.open as IsOpen, t.lastposterid as LastPosterUserId, t.postuserid as AuthorUserId, t.visible as IsVisible,
                    u.userid as Id, u.avatarrevision as AvatarRevision, u.username as UserName, u.posts AS TotalPosts, u.usertitle as UserTitle, u.lastactivity as LastActivityRaw, 
                    c.filename IS NOT NULL AS HasAvatar,
                    f.forumid as Id, f.title as Title,
                    g.usergroupid as Id, g.opentag as OpenTag, g.closetag as CloseTag, g.usertitle as UserTitle, g.adminpermissions as AdminPermissions
                FROM thread t
                LEFT JOIN user u ON (u.userid = t.lastposterid)
                LEFT JOIN forum f ON (f.forumid = t.forumid)
                LEFT JOIN usergroup g ON (g.usergroupid = u.usergroupid)
                LEFT JOIN customavatar c ON(c.userid = u.userid)";
        string postBaseQuery = @"
                SELECT p.postid AS Id, p.threadid AS ThreadId, p.parentid AS ParentPostId, p.dateline AS CreatedTimeRaw, p.pagetext AS TEXT, p.ipaddress AS IpAddress, p.visible AS VisibilityRaw, 
                        p.attach AS HasAttachments, p.post_thanks_amount AS ThanksCount,
                    u.userid AS Id, u.username AS UserName, u.usertitle AS UserTitle, u.avatarrevision AS AvatarRevision, u.lastactivity AS LastActivityRaw, c.filename IS NOT NULL AS HasAvatar,
                    g.usergroupid AS Id, g.opentag AS OpenTag, g.closetag AS CloseTag, g.usertitle AS UserTitle, g.adminpermissions AS AdminPermissions 
                FROM post p
                LEFT JOIN user u ON (u.userid = p.userid)
                LEFT JOIN usergroup g ON (u.usergroupid = g.usergroupid)
                LEFT JOIN customavatar c ON(c.userid = u.userid) ";
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
        string BuildUnreadActiveThreadsQuery(string selectFields, string additionalJoins = "", List<int> ignoredForumIds = null, List<int> ignoredThreadIds = null) {
            string sql = $@"
                SELECT {selectFields}
	            FROM post p
                INNER JOIN thread t ON(t.threadid = p.threadid)
                LEFT JOIN contentread r on(r.contenttypeid = 2 AND r.readtype = 'view' AND r.userid = p.userid and r.contentid = p.threadid)
                {additionalJoins}
	            WHERE t.lastposterid != p.userid
	            AND p.userid = @userId
	            AND (
		            t.lastpost > r.dateline OR (
			            r.readid IS NULL 
			            AND t.postuserid = p.userid 
			            AND t.lastpost > p.dateline
		            )
	            )
	            AND t.lastpost >= UNIX_TIMESTAMP(DATE_SUB(NOW(), INTERVAL @lastPostAgeDays DAY)) " +
            (ignoredForumIds != null ? "AND t.forumid NOT IN @ignoredForumIds " : "") +
            (ignoredThreadIds != null ? "AND t.threadid NOT IN @ignoredThreadIds " : "");

            return sql;
        }
        #endregion
        public VBLightThreadManager(MySqlConnection db, VBLightForumManager lightForumManager) {
            this.db = db;
            this.lightForumManager = lightForumManager;
        }

        public VBLightThread Get(int threadId, bool updateViews = true) {
            var args = new { threadId };
            string sql = threadBaseQuery + @"WHERE t.threadid = @threadId";
            // Generic overload not possible with QueryFirstOrDefault()
            var threads = db.Query(sql, threadMappingFunc, args);
            var thread = threads.SingleOrDefault();
            if (thread == null) {
                return null;
            }

            // FirstPost is fetched seperately because the query would be complex (especially for dapper) if we include the multiple joins from the post to its author/group here
            string firstPostSql = $@"{postBaseQuery} WHERE p.postid = @postId";
            thread.FirstPost = db.Query(firstPostSql, postMappingFunc, new { postId = thread.FirstPostId })
                .FirstOrDefault();

            if (updateViews) {
                string viewSql = @"
                    UPDATE thread
                    SET views = views + 1
                    WHERE threadid = @threadId";
                db.Execute(viewSql, new { threadId });
            }
            return thread;
        }

        /// <summary>
        /// Loads the replys of a thread for the page, which meta data were fetched by <see cref="VBLightThreadManager.GetReplysInfo(int, int, bool, int, int)"/>. 
        /// </summary>
        /// <param name="replysInfo"></param>
        /// <returns></returns>
        public List<VBLightPost> GetReplys(PageContentInfo replysInfo) {
            if (!replysInfo.ContentIds.Any()) {
                return new List<VBLightPost>();
            }

            return GetPosts(replysInfo.ContentIds);
        }
        /// <summary>
        /// Load posts by ids without any relation to a thread. If you want to fetch the replys for a specific thread page, use <see cref="GetReplys(PageContentInfo)"/>.
        /// </summary>
        public List<VBLightPost> GetPosts(List<int> postIds) {
            string sql = $@"
                {postBaseQuery}
                WHERE p.postid IN @postIds
                ORDER BY p.dateline";
            var replys = db.Query(sql, postMappingFunc, new { postIds });
            return replys.ToList();
        }

        /// <summary>
        /// Fetches the invisible posts between the first and last post of the page for moderator/administrator usage
        /// </summary>
        public List<VBLightPost> GetInvisibleReplys(int threadId, PageContentInfo replysInfo) {
            var args = new {
                threadId,
                firstPagePostId = replysInfo.ContentIds.FirstOrDefault(),
                lastPagePostId = replysInfo.ContentIds.LastOrDefault()
            };
            string sql = $@"
                {postBaseQuery}
                WHERE p.threadid = @threadId
                AND p.visible != 1 
                AND p.dateline >= (SELECT dateline FROM post WHERE postid = @firstPagePostId) " +
                (args.lastPagePostId != 0 ? "AND p.dateline <= (SELECT dateline FROM post WHERE postid = @lastPagePostId) " : "") + @"
                ORDER BY p.dateline";
            var replys = db.Query(sql, postMappingFunc, args);
            return replys.ToList();
        }
        /// <summary>
        /// Fetches meta information about the thread replys which are required to calculate paging. Returns only visible posts.
        /// </summary>
        /// <param name="includeDeleted">Include deleted posts for moderators or admin users</param>
        /// <param name="page">Number of the page to display, which is used for calculating which posts to skip</param>
        /// <param name="replysPerPage">How much replys should be present on a single page. VBulletins default is 9 (9 replys + firstpost = 19).</param>
        public PageContentInfo GetReplysInfo(int threadId, int threadFirstPostId, int page = 1, int replysPerPage = 10) {
            int offset = (page - 1) * replysPerPage;
            // The original replysPerPage is not touched for the pagination, since we don't count the posts rather than using VBs cache column for replys count
            int replysPerPageForPostIds = replysPerPage;
            // VB counts with the first posts per page, not only replys. To not break VBSeo links (which rely in the page) we skip one on the first page so that we have 10 posts instead of 11.
            if (page == 1) {
                replysPerPageForPostIds -= 1;
            } else {
                // For all other pages we need to get one post back. Otherwise we would skip the first post on the second page
                offset -= 1;
            }

            var postIdsArgs = new { threadId, offset, replysPerPageForPostIds, threadFirstPostId };
            var info = new PageContentInfo(page, replysPerPage);

            string sqlPostIds = $@"
                SELECT p.postid 
                FROM post p 
                WHERE p.threadId = @threadId 
                AND p.postid != @threadFirstPostId 
                AND p.visible = 1 
                ORDER BY p.dateline
                LIMIT @offset, @replysPerPageForPostIds";
            info.ContentIds = db.Query<int>(sqlPostIds, postIdsArgs)
                .ToList();

            var totalPagesArgs = new { threadId, replysPerPage };
            // +1 post is added to fix the skipped firstpost. Otherwise we missed the last page if it only contains a single post.
            // The if condition handles new threads without replys (avoids -1 / 10 calculation)
            string sqlTotalPages = @"
                SELECT IF(replycount = 0, 0, CEIL((replycount + 1) / @replysPerPage)) AS pages
                FROM thread
                WHERE threadId = @threadId";
            info.TotalPages = db.QueryFirstOrDefault<int>(sqlTotalPages, totalPagesArgs);
            return info;
        }

        /// <summary>
        /// Fetches the newest <paramref name="count"/> replys in a thread defined by <paramref name="threadId"/>
        /// </summary>
        public List<VBLightPost> GetNewestReplys(int threadId, int count = 10) {
            var args = new { threadId, count };
            string sql = $@"
                {postBaseQuery}
                WHERE p.threadid = @threadId
                AND p.visible = 1
                ORDER BY p.dateline DESC
                LIMIT @count";
            var replys = db.Query(sql, postMappingFunc, args);
            return replys.ToList();
        }
        /// <summary>
        /// Fetches the newest visible posts, without any grouping to the threads (if not threadId is specified). Usefull for polling, when you want to fetch new posts after a certain timestamp.
        /// </summary>
        /// <param name="afterTime">If this parameter is set, only posts with dateline > afterDateTime were fetched from the database</param>
        /// <param name="beforeTime">If this parameter is set, only posts with dateline less than beforeTime were fetched from the database</param>
        /// <param name="threadId">You could specify a thread id to only fetch replys from those thread (optional)</param>
        /// <param name="count">Limit the amout of data which is returned (default 10)</param>
        public List<VBLightPost> GetNewestPosts(DateTime? afterTime = null, DateTime? beforeTime = null, int? threadId = null,int count = 10) {
            var args = new {
                afterTimestamp = afterTime.HasValue ? afterTime.Value.ToUnixTimestamp() : 0,
                beforeTimestamp = beforeTime.HasValue ? beforeTime.Value.ToUnixTimestamp() : 0,
                threadId = threadId.HasValue ? threadId.Value : 0,
                count
            };
            string sql = $@"
                {postBaseQuery}
                WHERE p.visible = 1 " +
                (afterTime.HasValue ? " AND p.dateline > @afterTimestamp " : "") +
                (beforeTime.HasValue ? " AND p.dateline < @beforeTimestamp " : "") +
                (threadId.HasValue ? " AND p.threadid = @threadId " : "") + @"
                ORDER BY p.dateline DESC
                LIMIT @count";
            var replys = db.Query(sql, postMappingFunc, args);
            return replys.ToList();
        }
        /// <summary>
        /// Calculates the page of a specific post reply
        /// </summary>
        public int GetPageOfReply(int threadId, int replyId, int? threadFirstPostId = null, int replysPerPage = 10, bool includeDeleted = false) {
            var args = new { threadId, replyId, replysPerPage };
            // With the dateline we get 100% correct order. PostId should be working too in most cases, but not when posts were inserted with newer timestamp (e.g. import from other forum)
            // Fetching the entire post list can slow down the performance a bit on large threads (not much, < 100ms). This approach is faster by letting the sql server do the work

            // If we want to display 10 replys on every page (currently 9 on first page, 10 on every page > 1) we need to exclude the first post id here (currently not for compatibility reasons)
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
        /// <param name="minReplyCount">Filter for threads with a minimum amount of replys (larger or equal than this parameter)</param>
        /// <param name="orderByLastPostDate">If true, the threads were ordered by the date of their last reply. Otherwise the creation time of the thread is used.</param>
        /// <param name="includedForumIds">Optionally, you can pass a list of forum ids here to filter the threads. Only includedForumIds or excludedForumIds can be specified at once.</param>
        /// <param name="excludedForumIds">Optionally list of forum ids to exclude. Only includedForumIds or excludedForumIds can be specified at once.</param>
        /// <param name="afterTime">If set, only threads with a created dateline after this timestamp will be fetched</param>
        public List<VBLightThread> GetNewestThreads(int count = 10, int minReplyCount = 0, bool orderByLastPostDate = false, List<int> includedForumIds = null, List<int> excludedForumIds = null, DateTime? afterTime = null) {
            if (includedForumIds != null && excludedForumIds != null) {
                throw new Exception("Both includedForumIds and excludedForumIds are specified, which doesn't make sense. Please remote one attribute from the GetNewestThreads() call.");
            }
            var builder = new SqlBuilder();
            builder.Select(threadBaseQuery);

            if (includedForumIds?.Count > 0) {
                builder.Where("t.forumid IN @includedForumIds", new { includedForumIds });
            }
            if (excludedForumIds?.Count > 0) {
                builder.Where("t.forumid NOT IN @excludedForumIds", new { excludedForumIds });
            }
            if (afterTime.HasValue) {
                long afterTimestamp = afterTime.Value.ToUnixTimestamp();
                builder.Where("t.dateline > @afterTimestamp", new { afterTimestamp });
            }

            builder.Where("t.replycount >= @minReplyCount", new { minReplyCount });
            builder.OrderBy((orderByLastPostDate ? "t.lastpost" : "t.dateline") + " DESC");
            var builderTemplate = builder.AddTemplate("/**select**/ /**where**/ /**orderby**/ LIMIT @count", new { count });

            var result = db.Query(builderTemplate.RawSql, threadMappingFunc, builderTemplate.Parameters);
            return result.ToList();
        }

        /// <summary>
        /// Lists Threads with new replys from others in which the user was active (wrote at lest one post) ordered by last post time of the thread.
        /// Note that you should disable auto deletion of the contentread in VB to use this! Otherwise users will get already seen new replys to older threads because of the contentread table purge!
        /// </summary>
        /// <param name="userId">Id of the user to check for new replys in his active threads</param>
        /// <param name="count">Limit the amount of entries. Recommended since active users may get a larger set of data</param>
        /// <param name="lastPostAgeDays">Max age in days of the threads last post to filter out older threads without any activity. Default is 180 days (= last 6 months)</param>
        /// <param name="ignoredForumIds">Don't fetch notifications if they were posted in those forum ids </param>
        public List<VBLightUnreadActiveThread> GetUnreadActiveThreads(int userId, int count = 10, int lastPostAgeDays = 180, List<int> ignoredForumIds = null, List<int> ignoredThreadIds = null) {
            var args = new { userId, count, ignoredForumIds, ignoredThreadIds, lastPostAgeDays };

            // Grouping by contentid (which is the thread id) avoid returning a row for each post the user made in this thread.
            // This query fetches threads where the user wrote at least one post and new replys from other users were written since last read.
            // Since VB per default deletes the thread read by cron, we also check for threads created by the user containing new replys after the authors last post
            // ContentId 2 = Threads
            string selectFields = @"
                    t.threadid AS ThreadId, t.title AS ThreadTitle, t.lastpost AS LastPostTimeRaw,
				    f.forumid AS ForumId, f.title AS ForumTitle,
				    u.userid AS LastPosterUserId, u.avatarrevision AS LastPosterAvatarRevision, c.filename IS NOT NULL AS LastPosterHasAvatar,
				    r.readid";
            string additionalJoins = @"
	            LEFT JOIN forum f ON(f.forumid = t.forumid)
	            LEFT JOIN user u ON(u.userid = t.lastposterid)
                LEFT JOIN customavatar c ON(c.userid = u.userid) ";
            string sql = BuildUnreadActiveThreadsQuery(selectFields, additionalJoins, ignoredForumIds, ignoredThreadIds) + @"
                GROUP BY p.threadid 
                ORDER BY t.lastpost DESC
                LIMIT @count";
            var unreadThreads = db.Query<VBLightUnreadActiveThread>(sql, args);
            return unreadThreads.ToList();
        }

        /// <summary>
        /// Same as <see cref="GetUnreadActiveThreads(int, int, List{int}, List{int})"/> but this methods only count unread active threads without fetching any data for better performance
        /// </summary>
        public int CountUnreadActiveThreads(int userId, int lastPostAgeDays = 180, List<int> ignoredForumIds = null, List<int> ignoredThreadIds = null) {
            var args = new { userId, ignoredForumIds, ignoredThreadIds, lastPostAgeDays };
            string selectFields = "COUNT(DISTINCT p.threadid) AS cnt";
            string sql = BuildUnreadActiveThreadsQuery(selectFields, additionalJoins: "", ignoredForumIds, ignoredThreadIds);

            int count = db.QueryFirstOrDefault<int>(sql, args);
            return count;
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
        /// Gets the unix timestamp of the last readtime for a given content id of the given user. Usefull to resolve a "last unread post" threadlink
        /// </summary>
        public int GetContentReadTime(int contentId, int userId, int contentTypeId = 2, string readType = "view") {
            var args = new { contentId, userId, contentTypeId, readType };
            string sql = @"
                SELECT dateline
                FROM contentread
                WHERE contentid = @contentId AND userid = @userId AND contenttypeid = @contentTypeId AND readtype = @readType";
            int ts = db.QueryFirstOrDefault<int>(sql, args);
            return ts;
        }

        /// <summary>
        /// Gets the first unread post id after the provided timestamp. Can be used for building anker links to the first unread post since last threadread
        /// </summary>
        public int GetNextUnreadReplyId(int threadId, int lastReadTime) {
            var args = new { threadId, lastReadTime };
            string sql = @"
                SELECT postid
                FROM post
                WHERE threadid = @threadId and dateline >= @lastReadTime";
            int postId = db.QueryFirstOrDefault<int>(sql, args);
            return postId;
        }

        /// <summary>
        /// Gets received thanks from other users for the posts of the specified user id from "Post Thank you Hack" addon.
        /// For best performance, it's recommended to add an index (the table doesn't have any except on own id): ALTER TABLE post_thanks ADD INDEX userid_date(userid, DATE);
        /// </summary>
        /// <param name="userId">Id of the user that we should query for received thanks</param>
        /// <param name="afterTimestamp">If specified, only thanks after this timestamp are returned (optional)</param>
        /// <param name="count">Limit the number of thanks to return. Recommended since older/larger boards can return a massive amount of data if no limit is specified.</param>
        public List<VBLightPostThanks> GetThanks(int userId, int? afterTimestamp = null, int count = 10) {
            // Important to filter the UserId on Post (p) instead of the thanks! post_thanks.userid is the id of the user who gave the thanks, not the receiving user!
            string sql = $@"
                SELECT pt.date AS TimeRaw, pt.postid AS PostId,
			        t.threadid AS ThreadId, t.title AS ThreadTitle,
			        f.forumid AS ForumId, f.title AS ForumTitle,
                    u.username AS AuthorName,
                    g.opentag as AuthorGroupOpenTag, g.closetag as AuthorGroupCloseTag
                FROM post_thanks AS pt
                LEFT JOIN post AS p ON (p.postid = pt.postid)
                LEFT JOIN thread AS t ON (t.threadid = p.threadid)
                LEFT JOIN forum f ON(f.forumid = t.forumid)
                LEFT JOIN user u ON(u.userid = pt.userid)
                LEFT JOIN usergroup g ON(g.usergroupid = u.usergroupid)
                WHERE p.userid = @userId ";
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
            // We dont have any thanked posts if no post ids are present. Would cause broken conditions: WHERE p.postid IN (SELECT @postIds WHERE 1 = 0). Also better for performance.
            // This is just in case, since UL adds the first post id to this method call, so we always have at least one id to check (first post id on empty threads)
            if (!postIds.Any()) {
                return new List<int>();
            }

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
            if (!forumPermission.HasFlag(createOtherOrMyThreadsFlag)) {
                return CanReplyResult.NoReplyPermission;
            }
            return CanReplyResult.Ok;
        }

        /// <summary>
        /// Creates a new post reply to an existing thread and updates the thread as well as the corresponding forum with stats and meta info (last thread/timestamp etc)
        /// </summary>
        /// <param name="thread"><see cref="VBLightThread"></see> of <paramref name="replyModel"/> ThreadId. Optional to save one query in combination with 
        /// <see cref="VBLightThreadManager.CreateReply(LightCreateReplyModel)"</param>
        /// <param name="updateCounters">Determinates if the forums lastpost etc and authors post counter will be updated. Could be set to false if you want to do this with a cron insted.</param>
        /// <param name="updateThreadReplysCount">If true, the reply count cache in the thread table is updated. Set to false if the reply is the first post of a new created thread</param>
        /// <returns>Id of the created post</returns>
        public int CreateReply(LightCreateReplyModel replyModel, VBLightThread thread = null, bool updateCounters = true, bool updateThreadReplysCount = true) {
            if (thread == null) {
                thread = Get(replyModel.ThreadId);
            }

            var args = new {
                threadTitle = thread.Title,
                forumId = thread.Forum.Id,
                replyModel.ThreadId,
                thread.LastPostId,
                replyModel.Author.UserName,
                replyModel.Author.Id,
                replyModel.Title,
                replyModel.Text,
                replyModel.IpAddress
            };
            string updateCountersSql = @"
                UPDATE forum
                SET lastpost = @ts, lastposter = @userName, lastpostid = @postId, lastposterid = @id, lastthread = @threadTitle, lastthreadid = @threadId, replycount = replycount + 1
                WHERE forumid = @forumId; 

                UPDATE user
                SET posts = posts + 1
                WHERE userid = @id;";
            // ToDo: Support attachments
            string sql = @"
                START TRANSACTION;
                SELECT " + (replyModel.TimeRaw.HasValue ? replyModel.TimeRaw.Value.ToString() : "UNIX_TIMESTAMP()") + @" INTO @ts;

                INSERT INTO post
                SET threadid = @threadId, parentid = @lastPostId, username = @userName, userid = @id, title = @title, dateline = @ts, pagetext = @text, ipaddress = @ipAddress,
	                visible = 1, attach = 0;

                SELECT LAST_INSERT_ID() INTO @postId;
                UPDATE thread
                SET lastpostid = @postId,
                lastpost = @ts,
                lastposter = @userName,
                lastposterid = @id " +
                (updateThreadReplysCount ? ", replycount = replycount + 1" : "") + @"
                WHERE threadid = @threadId; " +

                (updateCounters ? updateCountersSql : "") + @"
                
                SELECT @postId;
                COMMIT;";
            int postId = db.QuerySingleOrDefault<int>(sql, args);
            return postId;
        }

        /// <summary>
        /// Creates a thread without performing any validation checks. Use UserGroupPermissions to check if the user can create threads in the specified forum
        /// </summary>
        /// <param name="updateCounters">Determinates if the forums lastpost etc and authors post counter will be updated. Could be set to false if you want to do this with a cron insted.</param>
        /// <returns></returns>
        public int CreateThread(LightCreateThreadModel threadModel, bool updateCounters = true) {
            long ts = DateTime.UtcNow.ToUnixTimestamp();

            var args = new {
                ts,
                threadModel.Title,
                threadModel.ForumId,
                threadModel.IsOpen,
                threadModel.Author.UserName,
                authorUserId = threadModel.Author.Id
            };
            string sql = @"
                START TRANSACTION;
                INSERT INTO thread 
                SET title = @title, lastpost = @ts, forumid = @forumId, open = @isOpen, postusername = @userName, postuserid = @authorUserId, lastposter = @userName, dateline = @ts, visible = 0, 
                    lastposterid = @authorUserId;

                SELECT LAST_INSERT_ID();
                COMMIT;";
            int threadId = db.QuerySingleOrDefault<int>(sql, args);
            if (threadId <= 0) {
                return -1;
            }

            var thread = Get(threadId);
            var replyModel = new LightCreateReplyModel(threadModel.Author, threadModel.ForumId, threadId, threadModel.Text, threadModel.IpAddress, timeRaw: ts, updateCounters: updateCounters);
            // VBs behaviour is to count only real replys without first post. This make sure that our overview can properly detect new threads without or with less replys.
            int postId = CreateReply(replyModel, updateThreadReplysCount: false);
            if (postId <= 0) {
                return -2;
            }

            var updateThreadArgs = new { ts, postId, threadId, threadModel.ForumId, threadModel.Author.UserName, threadModel.Author.Id, threadTitle = thread.Title };
            string updateThreadSql = @"
                UPDATE thread
                SET visible = 1, firstpostid = @postId, lastpostid = @postId
                WHERE threadid = @threadId;";
            // Dont update any forum/user counters here. This is already done centralized in AddReply!
            db.Execute(updateThreadSql, updateThreadArgs);
            return threadId;
        }

        #region Moderation
        /// <summary>
        /// Deletes a post and log the action in all logs as VB would do it (moderator and deletion log)
        /// </summary>
        public void DeletePost(VBLightPost post, VBLightUser moderator, string clientIp, string comment = "", bool softDelete = true) {
            if (!softDelete) {
                throw new NotImplementedException("Currently, hard deleting is not supported.");
            }

            db.Query("UPDATE post SET visible = 2 WHERE postid = @Id", new { post.Id });
            ReCountThreadReplys(post.ThreadId);

            var thread = Get(post.ThreadId);
            if (thread.LastPostId == post.Id) {
                UpdateLastPost(thread.Id);
            }

            LogModeratorAction(clientIp, moderator.Id, thread.Forum.Id, post.ThreadId, post.Id, post.Author.UserName);
            LogDeletion(post.Id, DeletionLogType.Post, moderator.Id, moderator.UserName, comment);
        }
        /// <summary>
        /// Logs moderator actions viewable in the VB Admin CP (e.g. deleted posts). In contrast to <see cref="LogDeletion(int, DeletionLogType, int, string, string)"/> this covers EVERY moderator action, not just deletions.
        /// </summary>
        public void LogModeratorAction(string clientIp, int moderatorUserId, int forumId, int threadId = 0, int postId = 0, string postAuthorName = "") {
            var action = new ArrayList() {
                // The first element is the title of the post. We don't care about it because it belongs to the thread
                "",
                postAuthorName
            };
            // VB provides array data as action and then serialize them using PHPs serialize/deserialize functions. PhpSerialization could serialize objects as PHP would do it
            var serializer = new PhpSerialization();
            string actionSerialized = serializer.Serialize(action);
            var actionSerializedSegments = actionSerialized.Replace(postAuthorName, "\n")
                .Split('\n');

            var modArgs = new {
                deletingUserId = moderatorUserId,
                postAuthorName,
                forumId,
                threadId,
                postId,
                clientIp
            };
            // Dapper cant resolve parameters in JSON strings like a:2:{i:0;s:19:"";i:1;s:6:"@postAuthorName";} correctly.
            // As a workaround, we just insert the username and then add the pre/suffix JSON using MySQLs CONCAT function around the username directly after inserting the modlog entry. Not really clean but currently we have no alternative.
            // ToDo: s:19 is not always constant, where s:6 at the end stay the same. Find out for what the first s:6 is used for
            string actionPrefix = actionSerializedSegments[0];
            string actionSuffix = actionSerializedSegments[1];
            string modSql = $@"INSERT INTO moderatorlog(dateline, userid, forumid, threadid, postid, action, type, ipaddress)
                VALUES(UNIX_TIMESTAMP(), @deletingUserId, @forumId, @threadId, @postId, '" + postAuthorName + "', 17, @clientIp);" +
                $"UPDATE moderatorlog SET action = CONCAT('{actionPrefix}', action, '{actionSuffix}') WHERE moderatorlogid = LAST_INSERT_ID();";
            db.Query(modSql, modArgs);
        }
        public void ReCountThreadReplys(int threadId) {
            string query = @"
                SELECT firstpostid
                INTO @firstPostId
                FROM thread
                WHERE threadid = @threadId;

                UPDATE thread
                    SET deletedcount = (
                        SELECT COUNT(*) 
                        FROM post 
                        WHERE threadid = @threadId 
                        AND visible = 2
                    ), replycount = (
                        SELECT COUNT(*) 
                        FROM post 
                        WHERE threadid = @threadId 
                        AND visible = 1
                        AND postid != @firstPostId
                    ), postercount = (
                        SELECT count(*)
                        FROM (
	                        SELECT distinct userid
	                        FROM post p
	                        WHERE p.threadid = @threadId
	                        AND p.visible = 1
	                        GROUP BY userid 
                        ) as x
                    )
                    WHERE threadid = @threadId;";
            db.Query(query, new { threadId });
        }
        /// <summary>
        /// Fetch deleted posts for the moderation deletion log. Designed to fetch the posts for a thread page. Doesn't include the posts itself.
        /// </summary>
        /// <param name="threadId">Id of the Thread</param>
        /// <param name="startPostTime">Timestamp of the first post on the page. Used as upper border (fetch posts AFTER this timestamp)</param>
        /// <param name="endPostTime">Timestamp of the last post on the page. Used as lower border (fetch posts BEFORE this timestamp). No border if set to 0 (for the last page)</param>
        public List<VBLightDeletionLog> GetDeletionLog(int threadId, int startPostTime, int endPostTime = 0, DeletionLogType type = DeletionLogType.Post) {
            string sql = @"
                SELECT p.dateline AS PostPublishTimeRaw, dl.primaryid AS ContentId, dl.type, dl.userid, dl.username, dl.reason, dl.dateline AS TimeRaw
                FROM post p, deletionlog dl
                WHERE p.postid = dl.primaryid
                AND p.threadid = @threadId
                AND p.visible != 1
                AND dl.type = @typeRaw
                AND p.dateline >= @startPostTime " +
               (endPostTime > 0 ? "AND p.dateline <= @endPostTime" : "");
            string typeRaw = type.ToString().ToLower();
            var args = new { threadId, startPostTime, endPostTime, typeRaw };
            var replys = db.Query<VBLightDeletionLog>(sql, args);
            return replys.ToList();
        }
        /// <summary>
        /// Fetches the last visible post from the DB and set those attribute (Id, Last poster name/id, ...) to the corresponding thread (cache columns)
        /// </summary>
        void UpdateLastPost(int threadId) {
            string lastPostSql = @"
                SELECT postid 
                FROM post
                WHERE threadid = @threadId
                AND visible = 1
                ORDER BY dateline DESC
                LIMIT 1";
            int newLastPostId = db.Query<int>(lastPostSql, new { threadId }).Single();
            var newLastPost = GetPost(newLastPostId);

            string lastPostIdSqlUpdate = @"
                    UPDATE thread
                    SET lastpostid = @newLastPostId,
                        lastpost = @postTimeRaw,
                        lastposter = @postAuthorName,
                        lastposterid = @postAuthorId
                    WHERE threadid = @threadId";
            var lastPostUpdateArgs = new {
                newLastPostId,
                threadId = newLastPost.ThreadId,
                postTimeRaw = newLastPost.CreatedTimeRaw,
                postAuthorName = newLastPost.Author.UserName,
                postAuthorId = newLastPost.Author.Id
            };
            db.Query(lastPostIdSqlUpdate, lastPostUpdateArgs);
        }
        /// <summary>
        /// Creates a deletion log entry. Used by vBulletin to display metadata in the thread (e.g. moderator name, comment). Kept private since we want to cover all VB actions where those log entries got created.
        /// </summary>
        void LogDeletion(int contentId, DeletionLogType type, int moderatorUserId, string moderatorUserName, string comment = "") {
            // Handy for optional ASP.NET Core attributes. If not set, they use null instead of an empty string, which cause trouble in our SQL queries.
            if (comment == null) {
                comment = "";
            }

            var args = new {
                contentId,
                type = type.ToString().ToLower(),
                moderatorUserId,
                moderatorUserName,
                comment
            };
            string sql = @"INSERT INTO deletionlog(primaryid, type, userid, username, reason, dateline)
                        VALUES(@contentId, @type, @moderatorUserId, @moderatorUserName, @comment, UNIX_TIMESTAMP())";
            db.Query(sql, args);
        }
        #endregion

        #region Attachments
        string attachmentsBaseQuery = @"
            SELECT attachmentid AS Id, a.userid AS UserId, a.dateline as TimeRaw, counter AS DownloadsCount, filename, a.contentid,
                    fd.filesize, fd.refcount 
            FROM attachment a, filedata fd";
        public List<VBLightAttachment> GetAttachments(List<int> postIds) {
            var builder = new SqlBuilder()
                .Select(attachmentsBaseQuery)
                .Where(@"a.filedataid = fd.filedataid
                        AND contentid IN @postIds", new { postIds });

            var builderTemplate = builder.AddTemplate("/**select**/ /**where**/");
            return db.Query<VBLightAttachment>(builderTemplate.RawSql, builderTemplate.Parameters).ToList();
        }
        public List<VBLightAttachment> GetAttachments(int postId) {
            return GetAttachments(new List<int> { postId });
        }

        public VBLightAttachment GetAttachment(int id) {
            var builder = new SqlBuilder()
                .Select(attachmentsBaseQuery)
                .Where(@"a.filedataid = fd.filedataid
                                    AND a.attachmentid = @id", new { id });

            var builderTemplate = builder.AddTemplate("/**select**/ /**where**/");
            return db.QueryFirstOrDefault<VBLightAttachment>(builderTemplate.RawSql, builderTemplate.Parameters);
        }
        #endregion
    }
}

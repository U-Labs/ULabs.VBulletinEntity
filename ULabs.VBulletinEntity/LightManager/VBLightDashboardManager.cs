using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using ULabs.VBulletinEntity.LightModels;

namespace ULabs.VBulletinEntity.LightManager {
    /// <summary>
    /// Collects data for a dashboard or homepage, where newest/unread content is requested
    /// </summary>
    public class VBLightDashboardManager {
        readonly MySqlConnection db;
        public VBLightDashboardManager(MySqlConnection db) {
            this.db = db;
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

            var args = new {
                includedForumIds = includedForumIds,
                excludedForumIds = excludedForumIds,
                count = count
            };
            Func<VBLightThread, VBLightUser, VBLightForum, VBLightThread> mappingFunc = (thread, user, forum) => {
                thread.LastPoster = user;
                thread.Forum = forum;
                return thread;
            };
            bool hasExclude = excludedForumIds != null || includedForumIds != null;
            bool hasWhere = hasExclude || onlyWithoutReplys;
            var threads = db.Query(@"
                    SELECT t.title as Title, t.threadid as ThreadId, t.lastpost as LastPostTimeRaw, t.lastposter as LastPosterName, t.lastposterid as LastPosterUserId, t.lastpostid as LastPostId, 
                        t.forumid as ForumId, t.replycount as ReplysCount,
                    u.userid as UserId, u.avatarrevision as AvatarRevision,
                    f.forumid as ForumId, f.title as Title
                    FROM thread t
                    LEFT JOIN user u ON (u.userid = t.lastposterid)
                    LEFT JOIN forum f ON (f.forumid = t.forumid)" +
                    (hasWhere ? "WHERE " : "") +
                    (includedForumIds != null ? "t.forumid IN @includedForumIds " : "") +
                    (excludedForumIds != null ? "t.forumid NOT IN @excludedForumIds " : "") +
                    (onlyWithoutReplys ? (hasExclude ? "AND " : "") + "t.replycount = 0 " : "") +
                    @"ORDER BY " + (orderByLastPostDate ? "t.lastpost " : "t.dateline ") +  @"DESC
                    LIMIT @count", mappingFunc, args, splitOn: "LastPosterUserId,ForumId");
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
            var args = new {
                userId,
                count,
                ignoredForumIds
            };

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
    }
}

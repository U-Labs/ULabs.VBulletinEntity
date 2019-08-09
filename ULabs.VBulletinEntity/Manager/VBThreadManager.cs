using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ULabs.VBulletinEntity.Caching;
using ULabs.VBulletinEntity.Models.AddOns;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Models.Permission;
using ULabs.VBulletinEntity.Models.User;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntity.Manager {
    public class VBThreadManager {
        readonly VBDbContext db;
        readonly VBForumManager forumManager;
        readonly VBUserManager userManager;
        readonly IVBCache cache;

        public VBThreadManager(VBDbContext db, VBForumManager forumManager, VBUserManager userManager, IVBCache cache) {
            this.db = db;
            this.forumManager = forumManager;
            this.userManager = userManager;
            this.cache = cache;
        }

        int[] PostVisiblityToIntArray(IEnumerable<VBPostVisibleState> visibility) {
            int[] visibilitysRaw = visibility.Select(x => (int)x)
                .ToArray();
            return visibilitysRaw;
        }

        public async Task<VBThreadRead> GetThreadReadAsync(int userId, int threadId) {
            var threadRead = await db.ThreadReads.SingleOrDefaultAsync(t => t.UserId == userId && t.ThreadId == threadId);
            return threadRead;
        }

        public async Task MarkThreadAsReadAsync(int userId, int threadId) {
            if (userId <= 0) {
                return;
            }

            var threadRead = await GetThreadReadAsync(userId, threadId);
            if (threadRead == null) {
                threadRead = new VBThreadRead(userId, threadId);
                await db.ThreadReads.AddAsync(threadRead);
            }

            threadRead.ReadTime = DateTime.Now;
            await db.SaveChangesAsync();
        }
        /// <summary>
        /// Get post visibilitys by user so that moderators can see deleted posts. If isModerator is null, this method checks wherever the user is global mod
        /// </summary>
        public async Task<List<VBPostVisibleState>> GetPostVisibleStatesAsync(VBSession session, bool? isModerator = false) {
            var postVisibilitys = new List<VBPostVisibleState> { VBPostVisibleState.Visible };
            if (session.LoggedIn) {
                if (!isModerator.HasValue) {
                    // ToDo: Check forum mod permission
                    isModerator = await userManager.IsGlobalModeratorAsync(session.User);
                }

                if (isModerator.Value) {
                    postVisibilitys.Add(VBPostVisibleState.Deleted);
                }
            }
            return postVisibilitys;
        }

        public async Task<int> CountReplysAsync(int threadId, IEnumerable<VBPostVisibleState> visibility) {
            var visibilitysRaw = PostVisiblityToIntArray(visibility);
            int replys = await db.Posts.CountAsync(p => p.ThreadId == threadId && visibilitysRaw.Contains(p.VisibilityRaw));
            return replys;
        }

        IQueryable<VBThread> GetSimpleThreadIncludeQuery(bool writeable = false) {
            var query = db.Threads.Include(t => t.FirstPost)
                // Removed for performance purpose: LastPost and LastPostAuthor are not required since we got them by Replys, which itself are loaded externally for paging purpose
                // Firstpost is required for ViewThread, since replys doesn't contain the thread authors post
                .Include(t => t.FirstPost)
                    // Required to show attachments in firstpost of thread
                    .ThenInclude(p => p.Attachments)
                .Include(t => t.FirstPost)
                    .ThenInclude(p => p.Author)
                        .ThenInclude(u => u.CustomAvatar)
                .Include(t => t.FirstPost)
                    // ToDo: Consider a second query if performance got worse by join
                    .ThenInclude(p => p.Author)
                    .ThenInclude(u => u.DisplayGroup)
                .Include(t => t.FirstPost)
                    .ThenInclude(p => p.Author)
                    .ThenInclude(u => u.UserGroup)
                .Include(t => t.Forum)
                .Include(t => t.Poll)
                .AsQueryable();

            if (!writeable) {
                query = query.AsNoTracking();
            }
            return query;
        }
        // Author is currently only present on read operations (ToDo)
        public async Task<VBThread> GetThreadAsync(int id, bool writeable = false) {
            // Don't want to use the cache for write operations since this may confuse EF's tracking system. So get a fresh copy for this cases
            if (!writeable && cache.TryGet(VBCacheKey.Thread, id.ToString(), out VBThread thread)) {
                return thread;
            }

            thread = await GetSimpleThreadIncludeQuery(writeable: writeable).SingleOrDefaultAsync(t => t.Id == id);
            if (thread.FirstPost != null) {
                // To avoid problems by confusing EFs change detection, we only add the author on write operations
                if (!writeable) {
                    thread.Author = thread.FirstPost.Author;
                }
            }
            cache.Set(VBCacheKey.Thread, id.ToString(), thread);
            return thread;
        }

        public async Task<Dictionary<VBForum, List<VBThread>>> GetCategoriesWithNewestThreadsAsync(VBUserGroup userGroup, List<int> ignoredCategoryForumIds = null, int countPerCategory = 8) {
            var dict = new Dictionary<VBForum, List<VBThread>>();
            var forums = await forumManager.GetCategoriesWithChildIdsAsync(userGroup, VBForumFlags.CanViewForum);
            var filteredForums = forums.AsEnumerable();

            if (ignoredCategoryForumIds != null) {
                filteredForums = filteredForums.Where(forumKvp => !ignoredCategoryForumIds.Contains(forumKvp.Key.Id))
                    .ToList();
            }

            foreach (var forumWithChilds in filteredForums) {
                var threads = await GetNewestThreadsAsync(forumWithChilds.Key.ChildList, count: countPerCategory);
                dict.Add(forumWithChilds.Key, threads);
            }
            return dict;
        }

        public async Task<VBPost> GetPostAsync(int id) {
            var post = await db.Posts.Include(p => p.Author)
                .ThenInclude(u => u.CustomAvatar)
                .FirstOrDefaultAsync(p => p.Id == id);
            return post;
        }
        /// <summary>
        /// For backward-compatibility, this method fetches replys based on start/end position for easy pagination
        /// Requires index ALTER TABLE `post` ADD INDEX `ul20_test_threadid_dateline_postid` (`threadid`, `dateline`, `postid`, `parentid`, `visible`);
        /// </summary>
        public async Task<List<VBPost>> GetReplysAsync(int threadId, IEnumerable<VBPostVisibleState> visibility = null, int start = 0, int count = 10) {
            string visibilityKey = visibility != null ? string.Join("|", visibility) : "";
            string secondKey = $"{threadId}.{visibilityKey}.{start}-{count}";

            if (cache.TryGet(VBCacheKey.ThreadReplys, secondKey, out List<VBPost> replys)) {
                return replys;
            }

            var query = GetReplyBaseQuery(threadId).Include(p => p.Author)
                    .ThenInclude(u => u.CustomAvatar)
                    .Include(p => p.Attachments)
                    .AsQueryable();

            // Directly fetching the posts using Skip() is very slow since temp tables are required. Using our method we fetch the ids from all replys from db and calculate the filter ids
            // using app server. The overhead is moderate: On a large thread with 118k replys it took ~150ms where only 2ms are required on regular threads with 112 replys. 
            if (start > 0) {
                // We only need this on page > 1: On the first page, simply fetch all replys
                var postIds = await GetPostIdsAsync(threadId, visibility);
                var currentPagePostIds = postIds.Skip(start)
                    .Take(count);
                query = query.Where(p => currentPagePostIds.Contains(p.Id));
            } else if (visibility != null) {
                // Only required on start > 0 since otherwise GetPostIds() handles visibility. If visibility is null, no filter is required.
                var visibilitys = PostVisiblityToIntArray(visibility);
                query = query.Where(p => visibilitys.Contains(p.VisibilityRaw));
            }

            replys = await query.Take(count)
            .ToListAsync();

            cache.Set(VBCacheKey.ThreadReplys, secondKey, replys);
            return replys;
        }
        /// <summary>
        /// Fetches the PostIds of all replys to avoid slowly temp tables when filtering with large join query on replay calculation by pagination
        /// </summary>
        public async Task<List<int>> GetPostIdsAsync(int threadId, IEnumerable<VBPostVisibleState> visibility = null) {
            int[] visibilitysRaw = null;
            if (visibility != null) {
                visibilitysRaw = PostVisiblityToIntArray(visibility);
            }
            // Using skip/start here is too slow
            var postIds = await db.Posts.Where(p => p.ThreadId == threadId && p.ParentPostId != 0)
                .Where(p => visibilitysRaw == null || visibilitysRaw.Contains(p.VisibilityRaw))
                .Select(p => p.Id)
                .ToListAsync();
            return postIds;
        }

        public async Task<int> GetNextUnreadReplyAsync(int threadId, DateTime lastRead) {
            var thread = await GetThreadAsync(threadId);
            // ToDo: When caching is introduced, we may simply use GetThread() to reuse cached data
            var replyMeta = await GetReplyBaseQuery(thread.Id)
                .Select(x => new {
                    x.Id,
                    x.CreatedTimeRaw
                }).ToListAsync();

            int nextUnreadId = replyMeta.Where(x => x.CreatedTimeRaw.ToDateTime().ForceUtc() > lastRead)
                .Select(x => x.Id)
                .FirstOrDefault();
            return nextUnreadId;
        }

        public async Task<int> GetPageOfReplyAsync(int threadId, int replyId, int replysPerPage) {
            var query = GetReplyBaseQuery(threadId);
            var replyIds = await query.Select(x => x.Id)
                .ToListAsync();

            int pos = replyIds.IndexOf(replyId) + 1;
            int page = (int)Math.Ceiling((decimal)pos / replysPerPage);
            return page;
        }

        IQueryable<VBPost> GetReplyBaseQuery(int threadId) {
            var query = db.Posts.Include(p => p.Author)
                        .ThenInclude(u => u.DisplayGroup)
                    .Include(p => p.Author)
                        .ThenInclude(u => u.UserGroup)
                // Do not use p.Thread.FirstPostId to exclude the thread enty post! Will slow down query extremly. ParentPostId is null on first post
                .Where(p => p.ThreadId == threadId && p.ParentPostId != 0)
                .OrderBy(p => p.CreatedTimeRaw);
            return query;
        }

        public async Task<List<VBThread>> GetNewestThreadsAsync(List<int> forumIds, List<int>ignoredForumIds = null, int offset = 0, int count = 10) {
            // Forum is required to build VBSEO like links in format {forumTitle}-{forumId}/{threadTitle}-{threadId}
            var threads = await db.Threads.Include(t => t.Forum)
                .Include(t => t.Author)
                    .ThenInclude(a => a.CustomAvatar)
                .Where(t => forumIds.Contains(t.ForumId) && (ignoredForumIds == null || !ignoredForumIds.Contains(t.ForumId)))
                .OrderByDescending(t => t.LastPostTimeRaw)
                .Skip(offset)
                .Take(count)
                .ToListAsync();
            return threads;
        }

        public async Task<List<PostThanks>> GetThanksAsync(int postId) {
            var thanks = await db.PostThanks.Include(t => t.User)
                    .ThenInclude(u => u.UserGroup)
                .Where(t => t.PostId == postId)
                .OrderByDescending(t => t.CreatedTimeRaw)
                .ToListAsync();
            return thanks;
        }
        public async Task CreateThanksAsync(int postId, int userId, string userName) {
            if (!db.PostThanks.Any(p => p.PostId == postId && p.UserId == userId)) {
                var thanks = new PostThanks() {
                    CreatedTime = DateTime.UtcNow,
                    PostId = postId,
                    UserId = userId,
                    UserName = userName
                };
                await db.PostThanks.AddAsync(thanks);
                // Not using GetPost here since we don't need all the join relations from that method and no cache is there yet
                var post = await db.Posts.FindAsync(postId);
                post.ThanksCount++;

                // If it's the first post of a thread, we need to clear the thread cache
                if (post.ParentPostId == 0) {
                    cache.Remove(VBCacheKey.Thread, post.ThreadId.ToString());
                } else {
                    // From GetReplysAsync - ToDo: update only changed data and manage that by a central caching manager
                    string cacheSubKeyPrefix = $"{post.ThreadId}.";
                    cache.Remove(VBCacheKey.ThreadReplys, cacheSubKeyPrefix);
                }

                await db.SaveChangesAsync();
            }
        }

        public async Task<List<int>> GetPostsWhereUserThankedAsync(int userId, List<int> postIds) {
            var thankedPostIds = await db.PostThanks.Where(t => t.UserId == userId && postIds.Contains(t.PostId))
                .Select(t => t.PostId)
                .ToListAsync();
            return thankedPostIds;
        }

        // ToDo: More additional parameters which can be passed to the constructor of Thread/Post class
        public async Task<VBThread> CreateThreadAsync(VBUser author, string authorIpAddress, int forumId, string title, string text) {
            // We check the forum first so that no thread is created when Forum id doesn't exist
            var forum = db.Forums.Find(forumId);
            if (forum == null) {
                throw new Exception($"No forum with id #{forumId.ToString()} exists!");
            }

            var post = new VBPost(author, title, text, authorIpAddress);
            db.Posts.Add(post);
            await db.SaveChangesAsync();

            var thread = new VBThread(post.AuthorId.Value, post.AuthorName, post.Id, post.Id, forumId, post.AuthorId.Value, post.AuthorName, DateTime.Now, DateTime.Now, title);
            db.Threads.Add(thread);
            await db.SaveChangesAsync();

            post.ThreadId = thread.Id;
            await db.SaveChangesAsync();

            var forum = db.Forums.Find(thread.ForumId);
            forum.LastPostId = post.Id;
            forum.LastThreadId = thread.Id;
            forum.LastThreadTitle = thread.Title;
            forum.PostCount++;
            forum.ThreadsCount++;
            forum.LastPostDate = DateTime.UtcNow;
            await db.SaveChangesAsync();

            await userManager.IncrementPostCounterAsync(author.Id, post.Id, post.CreatedTime);

            return thread;
        }

        public async Task<VBPost> CreateReplyAsync(VBUser author, int threadId, string text, string ipAddress, string title = "", bool updatePostCounter = true) {
            var thread = await db.Threads.FindAsync(threadId);
            var post = new VBPost(author, title, text, ipAddress, thread.Id);

            var lastPost = await db.Posts.Where(p => p.ThreadId == threadId)
                .OrderByDescending(p => p.CreatedTimeRaw)
                .FirstOrDefaultAsync();
            if (lastPost != null) {
                post.ParentPostId = lastPost.Id;
            }

            db.Posts.Add(post);
            await db.SaveChangesAsync();

            if (post.Id <= 0) {
                throw new Exception("Couldnt save post: No id generated from database!");
            }

            thread.LastPostAuthorId = author.Id;
            thread.LastPostAuthorName = author.UserName;
            thread.LastPostId = post.Id;
            thread.LastPostTime = DateTime.UtcNow;
            thread.ReplysCount++;
            // ToDo: Maybe also increment author counter if user hasn't posted before there

            bool userPostedInThread = await db.Posts.AnyAsync(p => p.ThreadId == thread.Id && p.AuthorId == author.Id);
            if (!userPostedInThread) {
                thread.PosterCount++;
            }

            await userManager.IncrementPostCounterAsync(author.Id, post.Id, post.CreatedTime);
            await db.SaveChangesAsync();

            cache.Remove(VBCacheKey.Thread, thread.Id.ToString());
            cache.Remove(VBCacheKey.ThreadReplys, thread.Id.ToString());
            return post;
        }
    }
}

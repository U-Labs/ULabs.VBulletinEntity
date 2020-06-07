using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using ULabs.VBulletinEntity.Manager;
using ULabs.VBulletinEntity.Models.Forum;
using ULabs.VBulletinEntity.Models.User;

namespace ULabs.VBulletinEntity.Tools {
    public static class DatabaseWarmUp {
        public static IWebHost WarmUp(this IWebHost webHost) {
            var serviceScopeFactory = (IServiceScopeFactory)webHost.Services.GetService(typeof(IServiceScopeFactory));

            using (var scope = serviceScopeFactory.CreateScope()) {
                var services = scope.ServiceProvider;

                var watch = Stopwatch.StartNew();
                WarmUpServices(services);
                watch.Stop();
                WriteColorized($"Database warmup took {watch.ElapsedMilliseconds} ms");
            }
            return webHost;
        }
        public static void WarmUpServices(VBDbContext db, VBThreadManager threadManager, VBSessionManager sessionManager, VBSettingsManager settingsManager, VBForumManager forumManager, VBUserManager userManager) {
            var attachment = db.Attachments.FirstOrDefault();
            var customAvatar = db.CustomAvatars.FirstOrDefault();
            var forumPerm = db.ForumPermissions.FirstOrDefault();
            var forum = db.Forums.FirstOrDefault();
            var message = db.Messages.FirstOrDefault();
            var messageText = db.MessagesText.FirstOrDefault();
            var poll = db.Polls.FirstOrDefault();
            var post = db.Posts.FirstOrDefault();
            var thanks = db.PostThanks.FirstOrDefault();
            var session = db.Sessions.FirstOrDefault();
            var setting = db.Settings.FirstOrDefault();
            var threadRead = db.ThreadReads.FirstOrDefault();
            var thread = db.Threads.FirstOrDefault();
            var group = db.UserGroups.FirstOrDefault();
            var user = db.Users.Include(u => u.UserGroup)
                .FirstOrDefault();

            WarmUpManagers(threadManager, sessionManager, settingsManager, forumManager, userManager, thread, user, session);
        }

        public static void WarmUpServices(IServiceProvider services) {
            var db = services.GetRequiredService<VBDbContext>();

            var threadManager = services.GetRequiredService<VBThreadManager>();
            var sessionManager = services.GetRequiredService<VBSessionManager>();
            var settingsManager = services.GetRequiredService<VBSettingsManager>();
            var forumManager = services.GetRequiredService<VBForumManager>();
            var userManager = services.GetRequiredService<VBUserManager>();
        }

        static void WarmUpManagers(VBThreadManager threadManager, VBSessionManager sessionManager, VBSettingsManager settingsManager, VBForumManager forumManager, VBUserManager userManager, VBThread thread, VBUser user, VBSession session) {
            var managerThread = threadManager.GetThreadAsync(thread.Id, writeable: true).Result;
            var replys = threadManager.GetReplysAsync(thread.Id, start: 0, count: 1).Result;
            var settings = settingsManager.GetCommonSettings();

            if (session != null) {
                // Do not update last activity since we're not in a http context => No request path avaliable
                var managerSession = sessionManager.GetAsync(session.SessionHash, updateLastActivity: false).Result;
            }

            if (user != null) {
                var forum = forumManager.GetCategoriesWhereUserCanAsync(user.UserGroup).Result;
                var randomUserFromManager = userManager.GetUserAsync(user.Id).Result;
            }
        }

        public static void WarmUpRequest(string warmUpPath) {
            var watch = Stopwatch.StartNew();
            string url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (url.Contains("0.0.0.0")) {
                url = url.Replace("0.0.0.0", "127.0.0.1");
            }

            Uri uri = new Uri(NormalizeUrl(url));
            var wc = (HttpWebRequest)WebRequest.Create(uri);
            wc.AllowAutoRedirect = true;
            wc.GetResponse();

            watch.Stop();
            WriteColorized($"Warmup request to to {uri} took {watch.ElapsedMilliseconds} ms");
        }

        static string NormalizeUrl(string url) {
            if (url.EndsWith("/")) {
                url = url.Substring(0, url.Length - 1);
            }
            return url;
        }

        static void WriteColorized(string txt, ConsoleColor color = ConsoleColor.Cyan) {
            lock (Console.Out) {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(txt);
                Console.ForegroundColor = oldColor;
            }
        }
    }
}

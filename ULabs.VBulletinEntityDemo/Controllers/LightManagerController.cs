﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ULabs.VBulletinEntity.Attributes;
using ULabs.VBulletinEntity.LightManager;
using ULabs.VBulletinEntity.LightModels.Forum;
using ULabs.VBulletinEntity.Manager;
using ULabs.VBulletinEntity.Models.Manager;
using ULabs.VBulletinEntity.Models.Permission;
using ULabs.VBulletinEntityDemo.Models;
using ULabs.VBulletinEntity.Tools;
using ULabs.VBulletinEntity.LightModels.User;
using System.Text;

namespace ULabs.VBulletinEntityDemo.Controllers {
    public class LightManagerController : Controller {
        readonly VBLightSessionManager lightSessionManager;
        readonly VBLightForumManager lightForumManager;
        readonly VBLightThreadManager lightThreadManager;
        readonly VBLightSettingsManager settingsManager;
        readonly VBLightUserManager lightUserManager;
        public LightManagerController(VBLightSessionManager lightSessionManager, VBLightForumManager lightForumManager, VBLightThreadManager lightThreadManager, VBLightSettingsManager settingsManager,
            VBLightUserManager lightUserManager) {
            this.lightSessionManager = lightSessionManager;
            this.lightForumManager = lightForumManager;
            this.lightThreadManager = lightThreadManager;
            this.settingsManager = settingsManager;
            this.lightUserManager = lightUserManager;

            var commonSettings = settingsManager.CommonSettings;
        }
        public IActionResult Dashboard() {
            var session = lightSessionManager.GetCurrent();
            var model = new LightDashboardModel(lightForumManager, lightThreadManager, lightSessionManager);
            var thx = lightThreadManager.GetThanks(userId: 18, afterTimestamp: 1566595766);
            //lightSessionManager.UpdateLastActivity(session.SessionHash, "/LightTest");
            var threadModel = new LightCreateThreadModel(session.User, forumId: 58, title: "Auto Testthread LightThreadManager", text: "Automatisch erzeugter Testthread", ipAddress: "127.0.0.1");
            //int tid = lightThreadManager.CreateThread(threadModel);
            int lastReadTs = lightThreadManager.GetContentReadTime(contentId: 38325, userId: 18);
            var lastReadDt = lastReadTs.ToDateTime();
            var dtLocal = lastReadDt.ToLocalTime();

            int unreadReplyId = lightThreadManager.GetNextUnreadReplyId(threadId: 38043, lastReadTime: 1566659655);
            int page = lightThreadManager.GetPageOfReply(threadId: 38043, replyId: 441387);
            var newestSmalltalkReplys = lightThreadManager.GetNewestReplys(threadId: 29780);

            var nonVisibleForumIds = lightForumManager.GetForumIdsWhereUserCanNot(1, VBForumFlags.CanViewForum);
            var newesttReplys = lightThreadManager.GetNewestReplys();
            var newesttReplysPublic = lightThreadManager.GetNewestReplys(excludedForumIds: nonVisibleForumIds);

            var adminTest = lightThreadManager.GetNewestThreads(8, minReplyCount: 1, excludedForumIds: new List<int>(), orderByLastPostDate: false);

            var allowedIds = lightForumManager.GetForumIdsWhereUserCan(session.User.PrimaryUserGroup.Id, VBForumFlags.CanViewForum);
            var forbidenIds = lightForumManager.GetForumIdsWhereUserCanNot(session.User.PrimaryUserGroup.Id, VBForumFlags.CanViewForum);
            return View(model);
        }
        public IActionResult ViewThread(int id, int page = 1) {
            var model = new ViewThreadModel(lightThreadManager, id, page, lightSessionManager.GetCurrent().User.Id);
            var invisibleReplys = lightThreadManager.GetInvisibleReplys(model.Thread.Id, model.ReplysInfo);
            return View(model);
        }

        public IActionResult Reply(int id) {
            var replyModel = new LightCreateReplyModel(lightSessionManager.GetCurrent().User, forumId: 73, threadId: id, text: "Testreply new light manager", ipAddress: "127.0.0.1");
            var check = lightThreadManager.CreateReplyCheck(replyModel);
            if (check == CanReplyResult.Ok) {
                int postId = lightThreadManager.CreateReply(replyModel);
                return Content("Check Ok");
            }
            return Content("Failed: " + check.ToString());
        }
        [VBLightAuthorize]
        public IActionResult DeleteFirstReply(int threadId) {
            var thread = lightThreadManager.Get(threadId);
            if (thread == null) {
                return Content($"Thread #{threadId} not found!");
            }

            var replyInfo = lightThreadManager.GetReplysInfo(thread.Id, thread.FirstPostId);
            var replys = lightThreadManager.GetReplys(replyInfo);
            var post = replys.FirstOrDefault();
            if (post == null) {
                return Content("No visible posts left in thread!");
            }

            var deletingUser = lightSessionManager.GetCurrent().User;
            lightThreadManager.DeletePost(post, deletingUser, "127.0.0.1", comment: "Test deletion by ULabs.VBulletinEntity Library");
            return Content($"Post #{post.Id} from {post.Author.UserName} deleted by mod {deletingUser.UserName}");
        }

        [VBLightAuthorize]
        public IActionResult ShowDeletedPost(int threadId, int startPostTime, int endPostTime) {
            var posts = lightThreadManager.GetDeletionLog(threadId, startPostTime, endPostTime);
            return Content($"{posts.Count} deleted Posts found: " + string.Join("<br>", posts.Select(p => p.UserName)));
        }
        public IActionResult GetForumThreads(int forumId, int page = 1) {
            var info = lightForumManager.GetForumThreadsInfo(forumId, page);
            var threads = lightForumManager.GetForumThreads(info);
            return View(threads);
        }
        public IActionResult GetNewestThreads() {
            var threads = lightForumManager.GetNewestThreads(orderByLastPostDate: true);
            return View(nameof(GetForumThreads), threads);
        }

        public IActionResult GetThread(int threadId) {
            var thread = lightThreadManager.Get(40358, updateViews: false);
            return Content(thread.Title);
        }
        public IActionResult ShowTopPosters() {
            var posters = lightUserManager.GetTopPostersFromCurrentMonth();
            return View(posters);
        }
        public IActionResult NewestContent() {
            const int count = 20;
            var threads = lightThreadManager.GetNewestThreads(count: count, excludedForumIds: new List<int>(), orderByLastPostDate: false);
            // Newest replys
            // var threads = lightThreadManager.GetNewestThreads(count: count, minReplyCount: 1, orderByLastPostDate: true);

            string resp = "";
            threads.ForEach(thread => resp += $"{thread.Title}<br>");
            return Content(resp, "text/html", Encoding.UTF8);
        }
        
        [VBLightAuthorize(permissionRedirectUrl: "/LightManager/Dashboard", requiredUserGroupId: 9)]
        public IActionResult Authorized() {
            return Content("Youre authorized!");
        }
    }
}
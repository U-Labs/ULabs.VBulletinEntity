using System;
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

            var adminTest = lightThreadManager.GetNewestThreads(8, minReplyCount: 1, excludedForumIds: new List<int>(), orderByLastPostDate: false);
            var pms = lightUserManager.GetReceivedPrivateMessages(userId: 18, VBPrivateMessageReadState.Unread, textPreviewWords: 1);
            var pms2 = lightUserManager.GetReceivedPrivateMessages(userId: 18, VBPrivateMessageReadState.Unread);
            var pms3 = lightUserManager.GetReceivedPrivateMessages(userId: 18);
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
            if(check == CanReplyResult.Ok) {
                int postId = lightThreadManager.CreateReply(replyModel);
                return Content("Check Ok");
            }
            return Content("Failed: " + check.ToString());
        }
        [VBLightAuthorize(permissionRedirectUrl: "/LightManager/Dashboard", requiredUserGroupId: 9)]
        public IActionResult Authorized() {
            return Content("Youre authorized!");
        }
    }
}
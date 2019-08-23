using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ULabs.VBulletinEntity.LightManager;
using ULabs.VBulletinEntity.LightModels.Forum;

namespace ULabs.VBulletinEntityDemo.Models {
    public class ViewThreadModel {
        public VBLightThread Thread { get; set; }
        public ReplysInfo ReplysInfo { get; set; }
        public List<VBLightPost> Replys { get; set; }
        public List<int> ThankedReplys { get; set; }

        public ViewThreadModel(VBLightThreadManager lightThreadManager, int threadId, int page, int userId) {
            Thread = lightThreadManager.Get(threadId);
            ReplysInfo = lightThreadManager.GetReplysInfo(Thread.Id, Thread.FirstPostId, page: page);
            Replys = lightThreadManager.GetReplys(ReplysInfo);
            ThankedReplys = lightThreadManager.GetPostsWhereUserThanked(userId, Replys.Select(p => p.Id).ToList());
        }
    }
}

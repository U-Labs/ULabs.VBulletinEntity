using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.Shared.Forum {
    public enum CanReplyResult {
        Ok,
        ThreadNotExisting,
        ThreadClosed,
        NoReplyPermission
    }
}

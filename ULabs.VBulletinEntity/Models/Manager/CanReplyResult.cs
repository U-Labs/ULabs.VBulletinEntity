using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.Models.Manager {
    public enum CanReplyResult {
        Ok,
        ThreadNotExisting,
        ThreadClosed,
        NoReplyPermission,
        TextEmpty
    }
}

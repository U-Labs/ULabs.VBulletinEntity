using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.Caching {
    public enum VBCacheKey {
        Settings, // Name of Entity class as second key
        UserSession,
        Thread, // Purging done (+ ThreadReplys)
        ThreadReplys, // Subkey: {id}.{start}-{count}
        Forums,

        LightSession,
        LightCommonSettings
    }
}

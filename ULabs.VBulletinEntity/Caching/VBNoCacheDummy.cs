using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.Caching {
    public class VBNoCacheDummy : IVBCache {
        public void Set(VBCacheKey key, object value, TimeSpan? expire = null) {
            
        }

        public void Set(VBCacheKey key, string subKey, object value, TimeSpan? expire = null) {
            
        }

        public bool TryGet<T>(VBCacheKey key, out T value) {
            value = default(T);
            return false;
        }

        public bool TryGet<T>(VBCacheKey key, string subKey, out T value) {
            value = default(T);
            return false;
        }

        public void Remove(VBCacheKey key, string subKeyPrefix) {
            
        }
    }
}

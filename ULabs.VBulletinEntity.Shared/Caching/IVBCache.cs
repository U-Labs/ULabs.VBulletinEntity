using System;

namespace ULabs.VBulletinEntity.Shared.Caching {
    public interface IVBCache {
        void Set(VBCacheKey key, object value, TimeSpan? expire = null);
        void Set(VBCacheKey key, string subKey, object value, TimeSpan? expire = null);

        bool TryGet<T>(VBCacheKey key, out T value);
        bool TryGet<T>(VBCacheKey key, string subKey, out T value);

        void Remove(VBCacheKey key, string subKeyPrefix);
    }
}

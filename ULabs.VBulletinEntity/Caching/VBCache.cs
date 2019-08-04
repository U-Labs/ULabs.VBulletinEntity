using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ULabs.VBulletinEntity.Caching {
    public class VBCache: IVBCache {
        readonly IMemoryCache memoryCache;
        readonly TimeSpan defaultTTL = TimeSpan.FromDays(7);
        // Must be static: We share the keys across ALL instances (ToDo: Consider injecting this class as singleton?)
        static HashSet<string> keys = new HashSet<string>();

        public VBCache(IMemoryCache memoryCache) {
            this.memoryCache = memoryCache;
        }

        void Set(string key, object value, TimeSpan? expire = null) {
            if (!expire.HasValue) {
                expire = defaultTTL;
            }
            keys.Add(key);
            memoryCache.Set(key, value, expire.Value);
        }

        public void Set(VBCacheKey key, object value, TimeSpan? expire = null) {
            Set(key.ToString(), value, expire);
        }

        public void Set(VBCacheKey key, string subKey, object value, TimeSpan? expire = null) {
            Set(CombineKey(key, subKey), value, expire);
        }

        bool TryGet<T>(string key, out T value) {
            return memoryCache.TryGetValue<T>(key, out value);
        }

        public bool TryGet<T>(VBCacheKey key, out T value) {
            return TryGet(key.ToString(), out value);
        }

        public bool TryGet<T>(VBCacheKey key, string subKey, out T value) {
            return TryGet(CombineKey(key, subKey), out value);
        }

        string CombineKey(VBCacheKey key, string subKey) {
            return $"{key.ToString()}.{subKey}";
        }

        public void Remove(VBCacheKey key, string subKeyPrefix) {
            string fullKey = CombineKey(key, subKeyPrefix);
            var matchingKeys = keys.Where(k => k.StartsWith(fullKey))
                .ToList();
            matchingKeys.ForEach(matchingKey => memoryCache.Remove(matchingKey));
        }
    }
}

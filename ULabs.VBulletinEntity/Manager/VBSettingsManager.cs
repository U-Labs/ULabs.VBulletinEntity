using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ULabs.VBulletinEntity.Caching;
using ULabs.VBulletinEntity.Models.Config;

namespace ULabs.VBulletinEntity.Manager {
    public class VBSettingsManager {
        readonly VBDbContext db;
        readonly IVBCache cache;

        public VBSettingsManager(VBDbContext db, IVBCache cache) {
            this.db = db;
            this.cache = cache;
        }

        public VBShowThreadSettings GetShowThreadSettings() {
            return GetSettingsWithCache<VBShowThreadSettings>();
        }

        public VBCommonSettings GetCommonSettings() {
            return GetSettingsWithCache<VBCommonSettings>();
        }

        T GetSettingsWithCache<T>() where T : new() {
            string cacheSubKey = typeof(T).FullName;
            if (cache.TryGet(VBCacheKey.Settings, cacheSubKey, out T settings)) {
                return settings;
            }

            settings = GetSettings<T>();
            cache.Set(VBCacheKey.Settings, cacheSubKey, settings, TimeSpan.FromDays(7));
            return settings;
        }

        T GetSettings<T>() where T : new() {
            var settings = new T();
            var props = settings.GetType()
                .GetProperties()
                // Column Attribute is required here to divide between real db properties and wrappers like CookieTimeout for CookieTimeoutRaw
                .Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute)))
                .ToList();
            var sqlKeys = props.Select(prop => GetAttributeFrom<ColumnAttribute>(settings, prop.Name).Name);
            var rawSettings = db.Settings.Where(setting => sqlKeys.Contains(setting.Name))
                .ToList();

            props.ForEach(prop => {
                var keyName = GetAttributeFrom<ColumnAttribute>(settings, prop.Name).Name;
                var val = rawSettings.SingleOrDefault(setting => setting.Name == keyName).StringValue;

                // ToDo: Handle other data types if required
                if (prop.PropertyType == typeof(int)) {
                    prop.SetValue(settings, int.Parse(val));
                } else {
                    prop.SetValue(settings, val);
                }
            });

            return settings;
        }

        T GetAttributeFrom<T>(object instance, string propertyName) where T : Attribute {
            var attrType = typeof(T);
            var property = instance.GetType().GetProperty(propertyName);
            return (T)property.GetCustomAttributes(attrType, false).First();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using ULabs.VBulletinEntity.Shared.Caching;
using ULabs.VBulletinEntity.Models.Config;
using ULabs.VBulletinEntity.Models.Config.AddOns;

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
        public VBRecycleBinSettings GetVBRecycleBinSettings() {
            return GetSettingsWithCache<VBRecycleBinSettings>();
        }

        public T GetSettingsWithCache<T>() where T : new() {
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
                // NotMapped divides between real db properties and wrappers like CookieTimeout for CookieTimeoutRaw
                .Where(prop => !Attribute.IsDefined(prop, typeof(NotMappedAttribute)))
                .Select(prop => {
                    // Convention: If we don't have a [Column] attribute, we use the property name in lowercase (same convention as for the db entity models)
                    string keyName = prop.Name.ToLower();
                    if (Attribute.IsDefined(prop, typeof(ColumnAttribute))) {
                        keyName = GetAttributeFrom<ColumnAttribute>(settings, prop.Name).Name;
                    }

                    return new KeyValuePair<string, PropertyInfo>(keyName, prop);
                })
                .ToDictionary(x => x.Key, y => y.Value);

            var sqlKeys = props.Select(prop => prop.Key);
            var rawSettings = db.Settings.Where(setting => sqlKeys.Contains(setting.Name))
                .ToList();

            foreach(var kvp in props) { 
                var val = rawSettings.SingleOrDefault(setting => setting.Name == kvp.Key).StringValue;
                var prop = kvp.Value;
                // ToDo: Handle other data types if required
                if (prop.PropertyType == typeof(int)) {
                    prop.SetValue(settings, int.Parse(val));
                } else if(prop.PropertyType == typeof(bool)) {
                    bool boolValue = val == "1";
                    prop.SetValue(settings, boolValue);
                }else {
                    prop.SetValue(settings, val);
                }
            }

            return settings;
        }

        T GetAttributeFrom<T>(object instance, string propertyName) where T : Attribute {
            var attrType = typeof(T);
            var property = instance.GetType().GetProperty(propertyName);
            return (T)property.GetCustomAttributes(attrType, false).First();
        }
    }
}

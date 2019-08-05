using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Caching;
using ULabs.VBulletinEntity.Manager;
using ULabs.VBulletinEntity.Models.Config;

namespace ULabs.VBulletinEntity {
    public static class VBServiceHelper {
        public static void AddVBDbContext<ICachingProvider>(this IServiceCollection services, VBConfig vbConfig, string connectionString, Version serverVersion = null, ServerType serverType = ServerType.MariaDb, bool sensitiveDataLogging = false)
            where ICachingProvider: IVBCache {
            services.AddSingleton(vbConfig);

            services.AddDbContext<VBDbContext>(options => {
                options.UseMySql(connectionString, mySqlOptions => mySqlOptions.ServerVersion(serverVersion, serverType));

                if (sensitiveDataLogging) {
                    options.EnableSensitiveDataLogging();
                }
            }, ServiceLifetime.Scoped);

            if(typeof(ICachingProvider) == typeof(VBCache)) {
                services.AddMemoryCache();
            }

            // https://stackoverflow.com/a/33567396/3276634
            services.AddScoped(typeof(IVBCache), typeof(ICachingProvider));
        }

        public static void AddVBManagers(this IServiceCollection services) {
            services.AddScoped<VBUserManager>();
            services.AddScoped<VBThreadManager>();
            services.AddScoped<VBForumManager>();
            services.AddScoped<VBSettingsManager>();
        }
    }
}

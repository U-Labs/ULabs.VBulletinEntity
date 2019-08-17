using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using ULabs.VBulletinEntity.Caching;
using ULabs.VBulletinEntity.LightManager;
using ULabs.VBulletinEntity.Manager;
using ULabs.VBulletinEntity.Models.Config;
using ULabs.VBulletinEntity.Tools;

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

            // For light managers
            services.AddScoped(x => new MySqlConnection(connectionString));

            if(typeof(ICachingProvider) == typeof(VBCache)) {
                services.AddMemoryCache();
            }

            // https://stackoverflow.com/a/33567396/3276634
            services.AddScoped(typeof(IVBCache), typeof(ICachingProvider));
        }

        public static void AddVBManagers(this IServiceCollection services, string vbCookieSalt, string vbCookiePrefix = "bb") {
            // Required to inject IHttpContextAccessor in VBUserManager so that we can fetch the users session. From package Microsoft.AspNetCore.Http
            // When missing: InvalidOperationException: Unable to resolve service for type 'Microsoft.AspNetCore.Http.IHttpContextAccessor' while attempting to activate 'ULabs.VBulletinEntity.Manager.VBUserManager'.
            services.AddHttpContextAccessor();
            services.AddScoped<VBSessionHelper>();

            services.AddScoped<VBUserManager>();
            services.AddScoped<VBThreadManager>();
            services.AddScoped<VBForumManager>();
            services.AddScoped<VBSettingsManager>();
            services.AddScoped<VBSessionManager>();
            services.AddScoped<VBAttachmentManager>();

            AddLightVBManagers(services, vbCookieSalt, vbCookiePrefix);
        }

        static void AddLightVBManagers(this IServiceCollection services, string vbCookieSalt, string vbCookiePrefix) {
            services.AddScoped<VBLightDashboardManager>();
            services.AddScoped<VBLightSettingsManager>();

            services.AddScoped(x => new VBLightSessionManager(x.GetRequiredService<IHttpContextAccessor>(), x.GetRequiredService<VBSessionHelper>(), x.GetRequiredService<MySqlConnection>(), vbCookieSalt, vbCookiePrefix));
        }
    }
}

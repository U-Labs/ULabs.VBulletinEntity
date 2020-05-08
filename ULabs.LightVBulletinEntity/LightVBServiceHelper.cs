using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using ULabs.LightVBulletinEntity.Config;
using ULabs.LightVBulletinEntity.LightManager;
using ULabs.VBulletinEntity.Shared.Caching;
using ULabs.VBulletinEntity.Shared.Tools;

namespace ULabs.LightVBulletinEntity {
    public static class LightVBServiceHelper {
        public static void AddLightVBDbContext<ICachingProvider>(this IServiceCollection services, VBConfig vbConfig, string connectionString, Version serverVersion = null, ServerType serverType = ServerType.MariaDb, bool sensitiveDataLogging = false)
            where ICachingProvider: IVBCache {
            services.AddSingleton(vbConfig);

            // For light managers
            services.AddScoped(x => new MySqlConnection(connectionString));

            if(typeof(ICachingProvider) == typeof(VBCache)) {
                services.AddMemoryCache();
            }

            // https://stackoverflow.com/a/33567396/3276634
            services.AddScoped(typeof(IVBCache), typeof(ICachingProvider));
        }

        public static void AddLightVBManagers(this IServiceCollection services, string vbCookieSalt, string vbCookiePrefix) {
            // Required to inject IHttpContextAccessor in VBUserManager so that we can fetch the users session. From package Microsoft.AspNetCore.Http
            // When missing: InvalidOperationException: Unable to resolve service for type 'Microsoft.AspNetCore.Http.IHttpContextAccessor' while attempting to activate 'ULabs.VBulletinEntity.Manager.VBUserManager'.
            services.AddHttpContextAccessor();
            services.AddScoped<VBSessionHelper>();

            services.AddScoped<VBLightSettingsManager>();
            services.AddScoped<VBLightForumManager>();
            services.AddScoped<VBLightThreadManager>();
            services.AddScoped<VBLightUserManager>();

            services.AddScoped(x => new VBLightSessionManager(x.GetRequiredService<IHttpContextAccessor>(), x.GetRequiredService<VBSessionHelper>(), x.GetRequiredService<VBLightSettingsManager>(),
                x.GetRequiredService<VBLightUserManager>(), x.GetRequiredService<MySqlConnection>(), x.GetRequiredService<ILogger<VBLightSessionManager>>(), x.GetRequiredService<IVBCache>(), 
                vbCookieSalt, vbCookiePrefix));
        }
    }
}

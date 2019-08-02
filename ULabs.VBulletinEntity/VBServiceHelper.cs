using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity {
    public static class VBServiceHelper {
        public static void AddVBDbContext(this IServiceCollection services, string connectionString, Version serverVersion = null, ServerType serverType = ServerType.MariaDb, bool sensitiveDataLogging = false) {
            services.AddDbContext<VBDbContext>(options => {
                options.UseMySql(connectionString, mySqlOptions => mySqlOptions.ServerVersion(serverVersion, serverType));

                if (sensitiveDataLogging) {
                    options.EnableSensitiveDataLogging();
                }
            }, ServiceLifetime.Scoped);
        }
    }
}

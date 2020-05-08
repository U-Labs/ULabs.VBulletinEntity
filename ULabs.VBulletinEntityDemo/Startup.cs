using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using ULabs.LightVBulletinEntity.Config;
using ULabs.VBulletinEntity;
using ULabs.VBulletinEntity.Models.Config;
using ULabs.VBulletinEntity.Tools;
using ULabs.LightVBulletinEntity;
using ULabs.VBulletinEntity.Shared.Caching;

namespace ULabs.VBulletinEntityDemo {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            var vbConfig = new VBConfig(Configuration.GetValue<string>("VBCookieSalt"));
            services.AddVBDbContext<VBCache>(vbConfig, Configuration.GetConnectionString("VBForum"), new Version(10, 3, 17), ServerType.MariaDb);
            services.AddVBManagers(vbConfig.CookieSalt);

            services.AddLightVBDbContext<VBCache>(vbConfig, Configuration.GetConnectionString("VBForum"), new Version(10, 3, 17), ServerType.MariaDb);
            services.AddLightVBManagers(vbConfig.CookieSalt, vbConfig.CookiePrefix);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLife) {
            appLife.ApplicationStarted.Register(() => DatabaseWarmUp.WarmUpRequest("WarmUp/Index"));

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            } else {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

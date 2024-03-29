﻿using System;
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
using ULabs.VBulletinEntity;
using ULabs.VBulletinEntity.Shared.Caching;
using ULabs.VBulletinEntity.Models.Config;
using ULabs.VBulletinEntity.Tools;
using Microsoft.Extensions.Hosting;

namespace ULabs.VBulletinEntityDemo {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            var vbConfig = new VBConfig(Configuration.GetValue<string>("VBCookieSalt"));
            services.AddVBDbContext<VBCache>(vbConfig, Configuration.GetConnectionString("VBForum"), ServerType.MariaDb);
            services.AddVBManagers(vbConfig.CookieSalt);
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLife) {
            appLife.ApplicationStarted.Register(() => DatabaseWarmUp.WarmUpRequest("WarmUp/Index"));

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            } else {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

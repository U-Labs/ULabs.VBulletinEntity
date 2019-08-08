using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntityDemo {
    public class Program {
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build()
                // Warmup database contexts and managers
                .WarmUp()
                .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}

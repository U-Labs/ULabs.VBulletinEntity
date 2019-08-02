# U-Labs VBulletin Entity
A vBulletin 4 database abstraction layer for Entity Framework Core - OSS under [GNU GPLv3](https://choosealicense.com/licenses/gpl-3.0/)

# Usage
ToDo: NuGet Package

## Configure Connection Strings
In your `appsettings.json` insert a [Connection String](https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-strings) for the
vBulletin MySQL database in the corresponding section like this:

```json
{
  "ConnectionStrings": {
    "VBForum": "Server=localhost;Database=vb_forum;User=vb_forum;Password=xxx;"
  },
  ...
}
```

Register the database services with the specified Connection String: 

```cs
using System;
using ULabs.VBulletinEntity.DatabaseExtensions;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ULabs.VBulletinEntityDemo {
    public class Startup {
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddVBDbContext(Configuration.GetConnectionString("VBForum"), new Version(10, 3, 17), ServerType.MariaDb);
            // ...
        }
    }
}
```

Tipp for local developing: Use `appsettings.Development.json` to override the Connection String. If `appsettings.Development.json` is added
to your `.gitignore`, no credentials can be checked in by accident. This approach is used in our example project.  
ASP.NET Core automatically adds files following the pattern `appsettings.{Environment}.json` when existing. 
So keep in mind, that this only works while `ASPNETCORE_ENVIRONMENT` is set to `Development`. 

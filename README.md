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

## Motivation
This project is part of my approach to develop on a modern .NET Core application stack for vBulletin. I did some POCs, also on the database.
But now it's time to create a better structure. Since I believe in open source and also use a lot of OSS, I'd also like to share my work to
give something back for the community.

## Contributions/Coding Conventions

Every help on this library is welcome! The code in this repository should fit to 
[the official C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions). 
My only intentionally deviation from this were the curly brackets, which are not placed in an extra line. So code should looke like 

```cs
if(true) {
	myClass.DoAction();
}
```

instead of 

```cs
if(true) 
{
	myClass.DoAction();
}
```

## Credits
This project itself uses the following external open source libraries to which I would like to express my gratitude:
* [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
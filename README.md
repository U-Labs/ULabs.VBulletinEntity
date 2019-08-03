# U-Labs VBulletin Entity
A vBulletin 4 database abstraction layer for Entity Framework Core - OSS under [GNU GPLv3](https://choosealicense.com/licenses/gpl-3.0/). 
It is based on the current V2.1 LTS release of Entity Framework Core.

# Feature state

| Entity  | State | Comment |
| ------------- | ------------- |
| VBUserGroup  | ![Partly implemented](https://u-img.net/img/5113Ab.png) | Forum & Admin permissions implemented, rest of the permissions missing
| VBUser | x | x

![Complete](https://u-img.net/img/8798Dw.png) Full completed 
![Partly implemented](https://u-img.net/img/5113Ab.png) Partly implemented
![Not implemented yet](https://u-img.net/img/2301Ja.png) Not implemented (yet)

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

Please see our [dedicated conventions documentation](./docs/conventions.md) related to C# coding style and also vBulletin. 

## Credits
This project itself uses the following external open source libraries to which I would like to express my gratitude:
* [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
* Icons from made by [Flaticons](https://www.flaticon.com/) by [Freepik](https://www.flaticon.com/authors/freepik),
[Maxim Basinski](https://www.flaticon.com/authors/maxim-basinski) 
licensed with [CCC 3.0 BY](http://creativecommons.org/licenses/by/3.0/)

## Disclaimer
I'm not associated with _MH Sub I, LLC dba vBulletin_, the company behind vBulletin community software. As a customer of vBulletin, 
a valid licence for U-Labs is present. This is a private project which uses the term _vBulletin_ only to describe it's functionality. 
I do not make the brand vBulletin my own.
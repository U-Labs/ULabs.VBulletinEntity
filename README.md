# U-Labs VBulletin Entity
A vBulletin 4 database abstraction layer for Entity Framework Core - OSS under [GNU GPLv3](https://choosealicense.com/licenses/gpl-3.0/). 
It is based on the current V2.1 LTS release of Entity Framework Core.

## Feature state
![Complete](https://u-img.net/img/8798Dw.png) Full completed 
![Partly implemented](https://u-img.net/img/5113Ab.png) Partly implemented
![Not implemented yet](https://u-img.net/img/2301Ja.png) Not implemented (yet)

### Entities
Every entity represents a database table. The following list shows which are already implemented/need some work. 

| Entity  | State | Comment |
| ------------- | ------------- | -------------
| VBSettings | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBThreadRead | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBThread | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBPost | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBPoll | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBForumPermission | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBForum | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBFileData | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBAttachment | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBMessage | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBMessageText | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBSession | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBCustomAvatar | ![Complete](https://u-img.net/img/8798Dw.png) |
| VBUserGroup  | ![Partly implemented](https://u-img.net/img/5113Ab.png) | Forum & Admin permissions implemented, rest of the permissions missing
| VBUser | ![Partly implemented](https://u-img.net/img/5113Ab.png) | Some enums need to figured out. A few values are unknown and implemented raw

### Settings
Settings are stored in a single table, while `VBSettingsManager` map them to entity models similar as the ones above. 

| Group model| State | Comment |
| ------------- | ------------- | -------------
| VBCommonSettings | ![Complete](https://u-img.net/img/8798Dw.png) | Designed as abstraction for setting groups

## Get started
ToDo: NuGet Package

### Configure Connection Strings
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
using ULabs.VBulletinEntity.Caching;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ULabs.VBulletinEntityDemo {
    public class Startup {
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddVBDbContext<VBNoCacheDummy>(Configuration.GetConnectionString("VBForum"), new Version(10, 3, 17), ServerType.MariaDb);
            // For using Managers, see above
            services.AddVBManagers();
            // ...
        }
    }
}
```

Tipp for local developing: Use `appsettings.Development.json` to override the Connection String. If `appsettings.Development.json` is added
to your `.gitignore`, no credentials can be checked in by accident. This approach is used in our example project.  
ASP.NET Core automatically adds files following the pattern `appsettings.{Environment}.json` when existing. 
So keep in mind, that this only works while `ASPNETCORE_ENVIRONMENT` is set to `Development`. 

### Caching
Performance can be improved by caching data received from the database. This results in a faster page load time and less server load. When using the managers,
you can enable in-memory caching by passing our cache-provider instead of the default dummy one: 

```cs
services.AddVBDbContext<VBCache>(Configuration.GetConnectionString("VBForum"), new Version(10, 3, 17), ServerType.MariaDb);
```

The `VBCache` provider simply uses 
[Microsoft.Extensions.Caching.Memory](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-2.1). So the data is 
cached in the memory of your application server. But you also could use e.g. Redis or any other caching system by implemeting a provider based 
on [IVBCache interface](file://ULabs.VBulletinEntity/Cache/IVBCache.cs).

**Important**

This cache is designed to work with our Manager abstractions. They care about invalidating the cache (e.g. post after modification). But it's not possible
to automatically detect changes from outside, even not the DbContext. So **don't use** caching if you do write querys using raw DbContext, a vBulletin
installation runs in parallel or any other software modifies the database from outside. 

## Usage
You can choose between two ways of accessing VB data: 

### [Low level DbContext](./ULabs.VBulletinEntity/VBDbContext.cs)

Working with a familar DbContext instance give maximum flexibility: Every query that EF can technically do is possible. But you have to care about 
lazy loading propertys by yourself. Same way for required fields (e.g. creation timestamps) on writing queries. Recommended for advanced users which 
a deeper knowledge in vBulletins database, if the managers reach their limit for the use-case. 

If you registered the service by calling `services.AddVBDbContext()` as described in the getting started guide, you're already done. 
Simply let .NET Core's DI inject the context to your controller like this:

```cs
namespace ULabs.VBulletinEntityDemo.Controllers {
    public class DbContextController : Controller {
        readonly VBDbContext db;

        public DbContextController(VBDbContext db) {
            this.db = db;
        }
    }
}
```

Now use the instance to write your LINQ queries. See the [NewestContentModel.cs](./ULabs.VBulletinEntityDemo/Models/NewestContentModel.cs) model
in our example project. It shows how to fetch the newest users, threads, posts and active sessions. Also relations are included, for example
the forum of a thread. 

### High level Managers

Our managers try to cover common use-cases for developing a .NET based board. This helps keeping VB and database related logic outside of your application
project. While the managers will extended if needed, they doesn't aim to cover every special use-case. 

Managers are registered by adding `services.AddVBManagers()` to our `Startup.ConfigureServices()` method. 
Currently this public repo contains the following managers:

* [`VBUserManager`](./ULabs.VBulletinEntity/Manager/VBUserManager.cs)
* [`VBThreadManager`](./ULabs.VBulletinEntity/Manager/VBThreadManager.cs)
* [`VBForumManager`](./ULabs.VBulletinEntity/Manager/VBForumManager.cs)
* [`VBSettingsManager`](./ULabs.VBulletinEntity/Manager/VBSettingsManager.cs)

Simply register the required service in e.g. a controller constructor. 

#### VBSettingsManager
VBulletin has a lot of settings, divided into multiple groups. Addons can create their own settings groups. All of them got stored in the `setting` 
table. To handle them in a clean and reuseable way, we use the `VBSettingsManager` to map every group to a entity model. The first one avaliable
is `VBCommonSettings`. Find some working examples in the [SettingsController](./ULabs.VBulletinEntityDemo/Controllers/SettingsController.cs).

## Motivation
This project is part of my approach to develop on a modern .NET Core application stack for vBulletin. I did some POCs, also on the database.
But now it's time to create a better structure. Since I believe in open source and also use a lot of OSS, I'd also like to share my work to
give something back for the community.

## Contributions/Coding Conventions

Please see our [dedicated conventions documentation](./docs/conventions.md) related to C# coding style and also vBulletin. 

## Addon support
Some Addons apply modifications on the database like for example the [post thanks addon](https://www.vbulletin.org/forum/showthread.php?t=231666). 
Commonly, the core-tables were extended by custom columns. My idea was to seperate this by inheritance. In this case, we had a `VBUser` entity 
that only contains core-attributes from VB itself. For the Addon we create a inherited entity that adds the new addon fields: 

```cs
namespace ULabs.VBulletinEntity.Models.AddOns {
    [Table("user")]
    public class VBPostThanksUser : VBUser {
        [Column("post_thanks_user_amount")]
        public int PostThanksCount { get; set; }

        [Column("post_thanks_thanked_posts")]
        public int ThankedPostsCount { get; set; }
        // ...
    }
}
```
Sadly this isn't possible yet since [EF Core forces _discriminator_ columns](https://stackoverflow.com/questions/52588922/force-inherited-classes-in-asp-net-core-entity-framework-core-to-dedicated-mysql).
In case of inheritance, EF Core creates a column called _discriminator_ that contains the entity type (`VBUser` or `VBPostThanksUser` in this case). 
This makes things complicated. 

[Until EF Core introduced _Table-per-Concret Type_ as a fix](https://github.com/aspnet/EntityFrameworkCore/issues/3170), I decided not to spend more time
on this . Instead, Addon support is keept at a absolute minimum. For us this is only the 
[common post thanks addon](https://www.vbulletin.org/forum/showthread.php?t=231666), which is heavily used on U-Labs since years. 
I'm open for ideas how we could seperate this better in the future. 

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
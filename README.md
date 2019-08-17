# U-Labs VBulletin Entity
A vBulletin 4 database abstraction layer for Entity Framework Core - OSS under [GNU GPLv3](https://choosealicense.com/licenses/gpl-3.0/). 
It is based on the current V2.1 LTS release of Entity Framework Core.

![Demo project preview](https://u-img.net/img/6729Br.png)

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

## Usage
Install [the official NuGet-Package ULabs.VBulletinEntity](https://www.nuget.org/packages/ULabs.VBulletinEntity):
```
# Visual Studio Package Manager Console
Install-Package ULabs.VBulletinEntity
# DotNet CLI (e.g. for VS Code)
dotnet add package ULabs.VBulletinEntity
```

### Configure Connection Strings for Service Injection
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
            var vbConfig = new VBConfig(Configuration.GetValue<string>("VBCookieSalt"));
            services.AddVBDbContext<VBNoCacheDummy>(vbConfig, Configuration.GetConnectionString("VBForum"), new Version(10, 3, 17), ServerType.MariaDb);
            // For using Managers, see above
            services.AddVBManagers();
            // ...
        }
    }
}
```

`VBConfig` specifies configuration options which are only present in the VB filesystem like Cookie salt or cookie prefix (default `bb`). This is 
currently _only used for session handling_. 

#### Cookie salt

You'll find this as `COOKIE_SALT` constant in `{vBulletinInstallationDir}/includes/functions.php` at line 34:

```php
define('COOKIE_SALT', 'xyz');
```
In this example, your salt is `xyz`. 

#### Cookie prefix

Required to fetch session cookies. Defined in `{vBulletinInstallationDir}/includes/config.php`:

```php
$config['Misc']['cookieprefix'] = 'bb';
```

It's not required to pass the prefix if it wasn't manually changed. 

#### Tipp for local developing:

Use `appsettings.Development.json` to override the Connection String. If `appsettings.Development.json` is added
to your `.gitignore`, no credentials can be checked in by accident. This approach is used in our example project.  
ASP.NET Core automatically adds files following the pattern `appsettings.{Environment}.json` when existing. 
So keep in mind, that this only works while `ASPNETCORE_ENVIRONMENT` is set to `Development`. 

### Caching
Performance can be improved by caching data received from the database. This results in a faster page load time and less server load. When using the managers,
you can enable in-memory caching by passing our cache-provider instead of the default dummy one: 

```cs
services.AddVBDbContext<VBCache>(vbConfig, Configuration.GetConnectionString("VBForum"), new Version(10, 3, 17), ServerType.MariaDb);
```

The `VBCache` provider simply uses 
[Microsoft.Extensions.Caching.Memory](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-2.1). So the data is 
cached in the memory of your application server. But you also could use e.g. Redis or any other caching system by implemeting a provider based 
on [IVBCache interface](file://ULabs.VBulletinEntity/Cache/IVBCache.cs).

#### Important

This cache is designed to work with our Manager abstractions. They care about invalidating the cache (e.g. post after modification). But it's not possible
to automatically detect changes from outside, even not the DbContext. So **don't use** caching if you do write querys using raw DbContext, a vBulletin
installation runs in parallel or any other software modifies the database from outside. 

## Data Access Layer
You can choose between three ways of accessing VB data: 

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
* [`VBSessionManager`](./ULabs.VBulletinEntity/Manager/VBSessionManager.cs)
* [`VBAttachmentManager`](./ULabs.VBulletinEntity/Manager/VBAttachmentManager.cs)

Simply register the required service in e.g. a controller constructor. 

#### VBSettingsManager
VBulletin has a lot of settings, divided into multiple groups. Addons can create their own settings groups. All of them got stored in the `setting` 
table. To handle them in a clean and reuseable way, we use the `VBSettingsManager` to map every group to a entity model. The first one avaliable
is `VBCommonSettings`. Find some working examples in the [SettingsController](./ULabs.VBulletinEntityDemo/Controllers/SettingsController.cs).

#### VBSessionManager
The Session-Manager can fetch sessions created by VB to integrate your custom .NET Core applications. `VBSessionManager.GetCurrentAsync()`
return a `VBSession` object with the related `VBUser` if he has an authenticated session. This _only works_ if the Kestrel Webserver from
ASP.NET Core has access to the cookies. So it's required that VBulletin and your .NET Core application were hosted on the same domain. If VB runs on a subdomain (forum.example.com), 
the Cookie-Domain must be set to the TLD (example.com) in VBs Cookie settings. 

This can be archived in the simplest way by running some XAMPP/LAMPP stack on `localhost` default configuration. When you sign in, cookies
were set for `localhost`. Now start the demo application and access it using http://localhost:5000 and SSO should work. 

#### VBThreadManager
The following examples assumes that a instance of `VBThreadManager` was injected as `threadManager`.

##### Create thread
```csharp
string userId = 1;
int forumId = 1;
string title = "Thread created by ULabs.VBulletinEntity";
string text = "This thread was automatically generated using .NET Core!";
string ip = "127.0.0.1";

var user = await userManager.GetUserAsync(userId);
var thread = await threadManager.CreateThreadAsync(user, ip, forumId, title, text);
```
`CreateThreadAsync()` throws an exception if the forum passed as `forumId` parameter doesn't exist. All timestamps are set to the current
UTC date. You can test this in the demo project with the following url: http://localhost:5000/thread/create?userId=1&forumId=1&title=TestApiThread&text=SomeText

##### Check permissions
We use `CheckAsync` suffixed methods to perform checks if a user is allowed to perform some action. Creating 
a reply would be such an example, where you want to check permissions before creating posts:

```csharp
var session = await sessionManager.GetCurrentAsync();
var replyModel = new CreateReplyModel(session.User, threadId: 1, text: "Some reply content", ipAddress: "127.0.0.1");
var replyCheck = await threadManager.CreateReplyCheckAsync(replyModel);

if(replyCheck == CanReplyResult.Ok) {
    var reply = await threadManager.CreateReplyAsync(replyModel);
}else {
    // Show some information about the error
}
```

### High level Light Managers
Offering entities with common needed properties where you can work with using LINQ is fine. 
On the other side, this costs performance. Depending on the use-case, it's not a problem if things take a few
ms more or less. But at U-Labs we have situations, where fast page loading time is required. To also cover this case,
I introduced _Light Managers_: They follow the idea of relatively high managers, but without the cost of too much 
ressources usage.

For this purpose, Light Managers doesn't query the database with LINQ to SQL. Instead, we use 
[Dapper](https://github.com/StackExchange/Dapper), a known high performance ORM for .NET and .NET Core. Use managers
if query performance is important, but you also want to have the comfort of high level apis. But please check if 
the Light Managers can fit our needs. They're designed for special use cases and can't be such flexible than
the regular managers. 

Since Light Managers are very new, we currently only have one: 

* [`VBLightDashboardManager`](./ULabs.VBulletinEntity/LightManager/VBLightDashboardManager.cs)

Note that currently no async support is present because of 
[a bug in Dapper](https://github.com/mysql-net/MySqlConnector/issues/523#issuecomment-399701445)
that automatically closes the connection after each query. 

## Application Warmup
[A _cold_ Database Context is much slower on the first usage than a _warm_ Context.](https://stackoverflow.com/questions/13250679/how-to-warm-up-entity-framework-when-does-it-get-cold). 
This thread is a bit older, but the general problem also applys to EF Core as well as other ORMs: On the first request, everything needs to
be initialized from scatch. So the first request can be relatively slow, where all following requests will be much faster. Since this is not good for user experience, it's a good idea to warm up our Database Context. By doing this, the first user gets a faster
experience. There are to ways of doing this. 

### Database queries

This method is relatively simple: When starting our application, we create a `DbContext` instance and do a few requests on different 
entity types. In the main method of `Program.cs` replace `CreateWebHostBuilder(args).Build().Run();` by

```cs
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntityDemo {
    public class Program {
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build()
                .WarmUp()
                .Run();
        }
        // ...
    }
}
```
and see the requests in our Kestrel console. 

### HTTP Request
More effective is doing some real requests. In the best case, we call a MVC action with database queries because it would warm up the 
database as well as the MVC framework. But it's a bit more work. First we create a controller (here called `WarmUp`) with corresponding 
view of the `Index` action:

```cs
using ULabs.VBulletinEntity.Tools;

public class WarmUpController : Controller {
    readonly VBDbContext db;
    readonly VBThreadManager threadManager;
    readonly VBSessionManager sessionManager;
    readonly VBSettingsManager settingsManager;
    readonly VBForumManager forumManager;
    readonly VBUserManager userManager;

    public WarmUpController(VBDbContext db, VBThreadManager threadManager, VBSessionManager sessionManager, VBSettingsManager settingsManager, VBForumManager forumManager, VBUserManager userManager) {
        this.db = db;
        this.threadManager = threadManager;
        this.sessionManager = sessionManager;
        this.settingsManager = settingsManager;
        this.forumManager = forumManager;
        this.userManager = userManager;
    }

    public ActionResult Index() {
        DatabaseWarmUp.WarmUpServices(db, threadManager, sessionManager, settingsManager, forumManager, userManager);
        return View();
    }
}
```

It's not required to render those data in the corresponding view. Something small that is generated dynamically like this would be fine: 

```cs
@DateTime.Now.ToString()
```

Now add a injection of `IApplicationLifetime` to `Startup.Configure()` and reqister the warmup request on the event that got fired
when your aplication is started:

```cs
using ULabs.VBulletinEntity.Tools;

namespace ULabs.VBulletinEntityDemo {
    public class Startup {
        // ...
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLife) {
            appLife.ApplicationStarted.Register(() => DatabaseWarmUp.WarmUpRequest("WarmUp/Index"));
            // ...
        }
    }
}
```

### Performance comparisation
| Method | Page render time (ms)
| ---  | --- 
| No warmup | 2119
| Database queries on startup only | 800
| HTTP Request only | 97
| Database queries on startup + HTTP Request | 88

All tests run on Windows 10 x64 with the index page of our demo application, which contains multiple SQL queries with joins. 
As you can see, the HTTP request with database queries is the most effective way which results in very fast pages. Combining it with the
database queries on startup, there is not a huge improvement.

#### Why using both methods? 

This is related to the application startup order: 

```cs
CreateWebHostBuilder(args).Build()
    .WarmUp()
    .Run();
```

As you can see, our warmup queries runs _before_ the WebServer got started. This will already pre-warmup the database part of our application.
The warm-up request to ``WarmUp/Index` is done _after_ this and would be finished faster. In a modern Kubernetes deployment, this affects
the pod readiness. In other words: Durin an update, we could keep the first page load as fast as possible by warming up as much as possible
until Kubernetes consider the pod as ready and route traffic to it. 

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

## Motivation
This project is part of my approach to develop on a modern .NET Core application stack for vBulletin. I did some POCs, also on the database.
But now it's time to create a better structure. Since I believe in open source and also use a lot of OSS, I'd also like to share my work to
give something back for the community.

## Contributions/Coding Conventions

Please see our [dedicated conventions documentation](./docs/conventions.md) related to C# coding style and also vBulletin. 

## Credits
This project itself uses the following external open source libraries to which I would like to express my gratitude:
* [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
* [Dapper](https://github.com/StackExchange/Dapper)
* Icons from made by [Flaticons](https://www.flaticon.com/) by [Freepik](https://www.flaticon.com/authors/freepik),
[Maxim Basinski](https://www.flaticon.com/authors/maxim-basinski) 
licensed with [CCC 3.0 BY](http://creativecommons.org/licenses/by/3.0/)

## Disclaimer
I'm not associated with _MH Sub I, LLC dba vBulletin_, the company behind vBulletin community software. As a customer of vBulletin, 
a valid licence for U-Labs is present. This is a private project which uses the term _vBulletin_ only to describe it's functionality. 
I do not make the brand vBulletin my own.
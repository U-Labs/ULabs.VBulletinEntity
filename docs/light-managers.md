# Light Managers
Light Managers are based on the light ORM framework Dapper. As well as the full managers, they create some higher level of abstraction
to the database - but with focus on performance. This results in less flexibility. Not everything is possible with them. Light Managers
focus on the core features with the aim to do them very quick. 

Please note that currently no async support is present because of 
[a bug in Dapper](https://github.com/mysql-net/MySqlConnector/issues/523#issuecomment-399701445)
that automatically closes the connection after each query. 

## Caching
I recently started implementing caching in the light managers, so there is not much support yet.

| Entity  | State | What is cached? |
| ------------- | ------------- | -------------
| VBLightSessionManager | ![Not implemented yet](https://u-img.net/img/2301Ja.png) | Currently disabled because it also caches unread pms/thanks, which caused outdated data on new items
| VBLightSettingsManager | ![Complete](https://u-img.net/img/8798Dw.png) | `VBLightCommonSettings` are cached for 7 days
| VBLightForumManager | ![Not implemented yet](https://u-img.net/img/2301Ja.png) | 
| VBLightThreadManager | ![Not implemented yet](https://u-img.net/img/2301Ja.png) | 
| VBLightUserManager | ![Not implemented yet](https://u-img.net/img/2301Ja.png) | 

![Complete](https://u-img.net/img/8798Dw.png) Full supported 
![Partly implemented](https://u-img.net/img/5113Ab.png) Partly implemented
![Not implemented yet](https://u-img.net/img/2301Ja.png) Not implemented (yet)

## Logging
Based on [ASP.NET Core's logging provider](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1), 
the light managers write logs for showing you more detailled information whats going on inside. To show then, it's required to 
set a suffient logging level in `appsettings.json`. The full name of the class (`{Namespace}.{ClassName}`) is use as key
like this: 

```json
  "Logging": {
    "LogLevel": {
      "Default": "warning",
      "ULabs.VBulletinEntity.LightManager.VBLightSessionManager": "debug",
      "ULabs.VBulletinEntity.Caching.VBCache":  "debug"
    }
  }
```
This would give you detailled debug logs for the `VBLightSessionManager` as well as every caching operations. 

Logging isn't implemented in all managers because it make more sense on e.g. a session manager that shows how a session is fetched 
(or not) like on the user manager, since the latter one couldn't log much usefull information as were already provided by
return values. 
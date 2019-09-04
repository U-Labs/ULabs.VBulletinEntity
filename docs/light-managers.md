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
| VBLightSessionManager | ![Complete](https://u-img.net/img/8798Dw.png) | Sessions on the `Get` method with session hash as sub key for 30 minutes TTL
| VBLightForumManager | ![Not implemented yet](https://u-img.net/img/2301Ja.png) | 
| VBLightSettingsManager | ![Not implemented yet](https://u-img.net/img/2301Ja.png) | 
| VBLightThreadManager | ![Not implemented yet](https://u-img.net/img/2301Ja.png) | 
| VBLightUserManager | ![Not implemented yet](https://u-img.net/img/2301Ja.png) | 

![Complete](https://u-img.net/img/8798Dw.png) Full supported 
![Partly implemented](https://u-img.net/img/5113Ab.png) Partly implemented
![Not implemented yet](https://u-img.net/img/2301Ja.png) Not implemented (yet)
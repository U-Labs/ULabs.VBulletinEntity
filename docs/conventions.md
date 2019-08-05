## General Conventions
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

## VBulletin entities
VBulletin itself uses lowercase letters without any delimiter as naming conventions. This isn't compatible with the .NET ones. So we try to map
this. For example, the PK is called `{EntityType}id` like `threadid` or `postid`. EF call the PK simple `Id`. In this case, we call the field
`Id` and map it to `threadid`:

```cs
public class VBThread {
    [Column("threadid")]
    public int Id { get; set; }
}
```

As you can see, the entitys itself got a `VB` prefix. This should avoid confusion with existing classes from .NET or other librarys. Especially
`VBThread` is a good example here: IF we simply call it `Thread`, it could conflict with 
[System.Threading.Thread](https://docs.microsoft.com/de-de/dotnet/api/system.threading.thread?view=netcore-2.1). 
Fore sure you could work around this by using the full qualified name including it's namespace, but this messes up the code. So a simple prefix
that make clear it's an vBulletin entity seems the better solution here. 

### _Raw_ data types
Another problem is the usage of low level data types like unix timestamps. Of course we could use this in C#. But the `DateTime` class 
from .NET Core make life much easier when working with those data. So I decided to also convert those values. If this is the case, the 
original property got a suffix called `Raw`. This is required to keep vB compatibility. It also gives you a freedom of choice, since it's possible
to use parsed DateTimes as well as raw unix timestamps.

Example:

```cs
public class VBThread {
    [Column("lastpost")]
    public int LastPostTimeRaw { get; set; }

    [NotMapped]
    public DateTime LastPostTime {
        get { return LastPostTimeRaw.ToDateTime(); }
        set { LastPostTimeRaw = DateTimeExtensions.ToUnixTimestampAsInt(value); }
    }
}
```
We also sometimes changed the namings a bit to make it more clear. The column `lastpost` for example could be interpreted as id of the last post 
or for the timestamp. Since vBulletin itself added some suffixes (e.g. lastpost**id**), I made this more consistent. 

### `[Column("xyz")]` annotations
Our `VBDbContext` automatically converts propertys to lowercase. So we don't need a column annotation if our property and the database column have
the same name and were only different by upper/lower case writing. Example: We have a database field called `avatarmaxsize`. Our model rewrite 
it to `AvatarMaxSize`. No annotation is required. 

We only need them, when renaming a property is required to fit our needs or simply make it more readable. For example, this is the case on all unix timestamp
columns. Using this conventions, no annotations were required in most attributes. They should only be used when required so they don't 
inflate the models unnecessarily.

### Settings Manager
Every settings group is an entity. But by the database structure, we can't simply query them as other entites. `VBSettingsManager` can fetch an complete
settings group (e.g. `VBCommonSettings`) in a single query. The following conventions apply here:
* Use `[NotMapped]` annotations for custom properties that doesn't exist in the databsae (e.g. timestamp to `DateTime` mapping fields)
* Property names got converted lowercase to `varname` of the `settings` table (`TosUrl` ~> `tosurl`)
* If the property doesn't match (e.g. by convention) to the `varname` field, add a `[Column("<varname>")]` annotation:
    ```cs
    [Column("contactusoptions")]
    public string ContactUsSubjectsRaw { get; set; }
    ```

### Naming of _VBulletin_
We generally write VBulletin with capital V and also capital B, since it's a proper name of the brand product. While `Db` is just an abbreviation
for `Database`, it's written in lowercase, as recommended for C#. 
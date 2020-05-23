This project is the preparation for dividing the EF part (non-lightweight) in a seperate project. 
Since we have different things that were used in both projects (e.g. timestamp to DateTime conversation), this shared 
project should hold them.

## Integration
To avoid having the overhead of another NuGet package (which doesn't make really sense here), it's linked and included 
in the NuGet build using [this method](https://stackoverflow.com/a/56018426/3276634). 

We need to add `PrivateAssets="all"` on the `ProjectReference` in the demo project. But not in the Community project,
which consumes this library using the `ULabs.VBulletinEntity` package. If `PrivateAssets="all"` is present in
`ULabs.Community`, we got an exception: 

>  Unhandled Exception: System.IO.FileNotFoundException: Could not load file or assembly 'ULabs.VBulletinEntity, Version=0.7.17.0, Culture=neutral, PublicKeyToken=null'. The system cannot find the file specified.

This means, that a normal reference is required in **external** projects like this:

    <PackageReference Include="ULabs.VBulletinEntity" Version="0.7.17" />